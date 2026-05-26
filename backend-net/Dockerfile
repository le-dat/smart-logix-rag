# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project file first to restore dependencies (leverages Docker layer cache)
COPY SmartLogix.WebApi/SmartLogix.WebApi.csproj SmartLogix.WebApi/
RUN dotnet restore SmartLogix.WebApi/SmartLogix.WebApi.csproj

# Copy the rest of the source code and build/publish
COPY SmartLogix.WebApi/ SmartLogix.WebApi/
WORKDIR /src/SmartLogix.WebApi
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Configure standard ASP.NET Core container ports
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartLogix.WebApi.dll"]
