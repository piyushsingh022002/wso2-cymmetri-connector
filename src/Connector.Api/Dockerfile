# --------------------------
# BUILD STAGE
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy csproj and restore (for caching)
COPY *.csproj ./
RUN dotnet restore

# Copy all files
COPY . .

# Publish the app in Release mode
RUN dotnet publish -c Release -o /app/publish --no-restore

# --------------------------
# RUNTIME STAGE
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 80 (for Docker container)
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "Connector.Api.dll"]
