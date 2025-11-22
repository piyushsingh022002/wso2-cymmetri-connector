namespace Connector.Core.Models { 


public class CanonicalUser
{
public string Source { get; set; } = "wso2";
public string SourceId { get; set; } = null!; // WSO2 id
public string Username { get; set; } = null!;
public string GivenName { get; set; } = null!;
public string FamilyName { get; set; } = null!;
public string Email { get; set; } = null!;
}
}