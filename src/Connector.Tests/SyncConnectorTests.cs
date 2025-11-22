using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Connector.Connectors.Cymmetri;
using Connector.Connectors.Wso2;
using Connector.Core.Models;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

public class SyncConnectorTests
{
    [Fact]
    public async Task Wso2Client_WhenValidScimResponse_ReturnsMappedUsers()
    {
        // Arrange
        var scimJson = """
        {
          "Resources": [
            {
              "id": "abc123",
              "userName": "jdoe",
              "name": { "givenName": "John", "familyName": "Doe" },
              "emails": [ { "value": "jdoe@example.com", "primary": true } ]
            }
          ]
        }
        """;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/scim2/Users*")
            .Respond("application/json", scimJson);

        var client = new Wso2Client(new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost") });

        // Act
        var users = await client.GetUsersAsync();

        // Assert
        users.Should().HaveCount(1);
        var user = users[0];
        user.SourceId.Should().Be("abc123");
        user.Username.Should().Be("jdoe");
        user.GivenName.Should().Be("John");
        user.FamilyName.Should().Be("Doe");
        user.Email.Should().Be("jdoe@example.com");
    }

    [Fact]
    public async Task CymmetriClient_WhenCreatingUser_SendsCorrectPayload()
    {
        // Arrange
        var user = new CanonicalUser
        {
            SourceId = "abc123",
            Username = "jdoe",
            GivenName = "John",
            FamilyName = "Doe",
            Email = "jdoe@example.com"
        };

        string? capturedJson = null;
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, "/scim2/Users")
            .Respond(req =>
            {
                capturedJson = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

        var client = new CymmetriClient(new HttpClient(mockHttp) { BaseAddress = new Uri("http://localhost") });

        // Act
        var response = await client.CreateUserAsync(user);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        capturedJson.Should().NotBeNull();

        using var doc = JsonDocument.Parse(capturedJson!);
        var root = doc.RootElement;
        root.GetProperty("userName").GetString().Should().Be("jdoe");
        root.GetProperty("name").GetProperty("givenName").GetString().Should().Be("John");
        root.GetProperty("name").GetProperty("familyName").GetString().Should().Be("Doe");
        root.GetProperty("emails")[0].GetProperty("value").GetString().Should().Be("jdoe@example.com");
        root.GetProperty("externalId").GetString().Should().Be("abc123");
    }

    [Fact]
    public async Task SyncWorker_WhenUsersExist_CallsCreateUserForEach()
    {
        // Arrange
        var users = new[]
        {
            new CanonicalUser { SourceId = "1", Username = "a", GivenName = "A", FamilyName = "B", Email = "a@b.com" },
            new CanonicalUser { SourceId = "2", Username = "b", GivenName = "C", FamilyName = "D", Email = "c@d.com" }
        };

        var wso2Mock = new Mock<Wso2Client>(MockBehavior.Strict, new HttpClient(new MockHttpMessageHandler()));
        wso2Mock.Setup(x => x.GetUsersAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(users.ToList());

        var cymMock = new Mock<CymmetriClient>(MockBehavior.Strict, new HttpClient(new MockHttpMessageHandler()));
        cymMock.Setup(x => x.CreateUserAsync(It.IsAny<CanonicalUser>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var worker = new SyncWorker(wso2Mock.Object, cymMock.Object);

        // Act
        // Call ExecuteAsync but cancel after first iteration to avoid loop
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10); // Cancel quickly after start

        await worker.ExecuteAsync(cts.Token);

        // Assert
        cymMock.Verify(x => x.CreateUserAsync(It.IsAny<CanonicalUser>()), Times.Exactly(2));
    }
}