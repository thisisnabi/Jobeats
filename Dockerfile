FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Jobeats.sln ./
COPY src/Jobeats.Core/*.csproj src/Jobeats.Core/
COPY src/Jobeats.Application/*.csproj src/Jobeats.Application/
COPY src/Jobeats.Infrastructure/*.csproj src/Jobeats.Infrastructure/
COPY src/Jobeats.Api/*.csproj src/Jobeats.Api/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/

# Build
RUN dotnet build src/Jobeats.Api/Jobeats.Api.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish src/Jobeats.Api/Jobeats.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create data directory
RUN mkdir -p /app/data

# Copy published app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/jobeats.db"

ENTRYPOINT ["dotnet", "Jobeats.Api.dll"]
