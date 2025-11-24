# ============================
# BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire solution
COPY . .

# Restore only the API project (this restores all referenced projects)
RUN dotnet restore src/Connector.Api/Connector.Api.csproj

# Publish API
RUN dotnet publish src/Connector.Api/Connector.Api.csproj -c Release -o /app/publish

# ============================
# RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Connector.Api.dll"]
