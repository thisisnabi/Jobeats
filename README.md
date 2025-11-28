# Jobeats - Health Check Monitoring System

Jobeats is a comprehensive cron job and background task monitoring service built with .NET 9. It monitors scheduled tasks by listening for HTTP "pings" and sends alerts when pings don't arrive on time.

Inspired by [healthchecks.io](https://healthchecks.io/), Jobeats provides a self-hosted solution for monitoring your scheduled jobs, cron tasks, and background processes.

## Features

- **Simple Ping API** - Monitor jobs by sending HTTP pings
- **Multiple Ping Types** - Success, Failure, Start, Log, and Exit Status pings
- **Run ID Tracking** - Support for concurrent execution monitoring
- **Flexible Scheduling** - Period-based or cron expression schedules
- **Grace Periods** - Configure how long to wait before alerting
- **Status Tracking** - Real-time status (new, up, down, paused, started)
- **Multiple Notification Channels**:
  - Email
  - Slack
  - Discord
  - Webhooks
- **RESTful API** - Full CRUD operations for checks
- **Background Processing** - Automatic alert processing and cleanup
- **Health Endpoint** - Monitor the service itself

## Quick Start

### Using Docker

```bash
# Clone the repository
git clone https://github.com/thisisnabi/Jobeats.git
cd Jobeats

# Build and run with Docker Compose
docker-compose up -d

# The API will be available at http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

### Using .NET CLI

```bash
# Clone the repository
git clone https://github.com/thisisnabi/Jobeats.git
cd Jobeats

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run the API
dotnet run --project src/Jobeats.Api

# The API will be available at http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

## API Usage

### Pinging API

Your monitored jobs should send HTTP requests to the ping endpoints:

```bash
# Success ping (job completed successfully)
curl https://your-domain/ping/{check-uuid}

# Failure ping (job failed)
curl https://your-domain/ping/{check-uuid}/fail

# Start signal (job started, starts timer)
curl https://your-domain/ping/{check-uuid}/start

# Log-only ping (doesn't affect status)
curl https://your-domain/ping/{check-uuid}/log

# Exit status ping (0 = success, 1-255 = failure)
curl https://your-domain/ping/{check-uuid}/0

# With run ID for concurrent execution tracking
curl https://your-domain/ping/{check-uuid}?rid=run-123
```

### Management API

All management endpoints require an API key in the `X-Api-Key` header:

```bash
# List all checks
curl -H "X-Api-Key: your-api-key" https://your-domain/api/v1/checks

# Create a new check
curl -X POST -H "X-Api-Key: your-api-key" \
     -H "Content-Type: application/json" \
     -d '{"name": "Daily Backup", "timeout": 86400, "grace": 3600}' \
     https://your-domain/api/v1/checks

# Get check details
curl -H "X-Api-Key: your-api-key" https://your-domain/api/v1/checks/{uuid}

# Update a check
curl -X POST -H "X-Api-Key: your-api-key" \
     -H "Content-Type: application/json" \
     -d '{"name": "Updated Name"}' \
     https://your-domain/api/v1/checks/{uuid}

# Delete a check
curl -X DELETE -H "X-Api-Key: your-api-key" \
     https://your-domain/api/v1/checks/{uuid}

# Pause monitoring
curl -X POST -H "X-Api-Key: your-api-key" \
     https://your-domain/api/v1/checks/{uuid}/pause

# Resume monitoring
curl -X POST -H "X-Api-Key: your-api-key" \
     https://your-domain/api/v1/checks/{uuid}/resume

# Get ping history
curl -H "X-Api-Key: your-api-key" \
     https://your-domain/api/v1/checks/{uuid}/pings

# Get flip (status change) history
curl -H "X-Api-Key: your-api-key" \
     https://your-domain/api/v1/checks/{uuid}/flips
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQLite connection string | `Data Source=jobeats.db` |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=jobeats.db"
  },
  "Jobeats": {
    "PingLogLimit": 100,
    "AlertCheckIntervalSeconds": 30,
    "PruneIntervalHours": 1
  }
}
```

## Architecture

```
src/
├── Jobeats.Api/           # ASP.NET Core Web API
├── Jobeats.Core/          # Domain models, interfaces
├── Jobeats.Application/   # Business logic, CQRS handlers
└── Jobeats.Infrastructure/# EF Core, notifications, background services

tests/
├── Jobeats.UnitTests/
└── Jobeats.IntegrationTests/
```

### Design Patterns

- **Clean Architecture** - Separation of concerns
- **CQRS** - Command Query Responsibility Segregation using MediatR
- **Repository Pattern** - Data access abstraction
- **Strategy Pattern** - For different notification channels

### Technology Stack

- **.NET 9** - Core framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM with SQLite
- **MediatR** - CQRS and messaging
- **FluentValidation** - Input validation
- **xUnit** - Testing framework
- **Moq** - Mocking framework

## Domain Models

### Check
Represents a monitored job with:
- Name, description, tags
- Period (expected time between pings)
- Grace time (how long to wait before alerting)
- Status (new, up, down, paused, started)
- Optional cron expression

### Ping
Records each ping received:
- Type (success, fail, start, log, ignored)
- Timestamp, user agent, remote IP
- Optional request body
- Run ID for concurrent execution tracking
- Duration (for start → success tracking)

### Flip
Records status changes:
- Old status → New status
- Timestamp
- Used for triggering notifications

### Project
Groups checks together:
- API key for authentication
- Owner and team members

### Channel
Notification configuration:
- Type (email, slack, discord, webhook)
- Configuration JSON
- Delivery metrics

## Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/Jobeats.UnitTests

# Run integration tests only
dotnet test tests/Jobeats.IntegrationTests
```

## Development

### Prerequisites

- .NET 9 SDK
- Docker (optional, for containerized deployment)

### Building

```bash
dotnet build
```

### Running in Development

```bash
dotnet run --project src/Jobeats.Api
```

## Health Endpoint

The service exposes a `/health` endpoint for monitoring:

```bash
curl http://localhost:8080/health
```

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- Inspired by [healthchecks.io](https://github.com/healthchecks/healthchecks)
- Built with [.NET](https://dotnet.microsoft.com/)
