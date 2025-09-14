
````markdown name=README.md (SSO-BE)
# SSO-BE

This is the backend for the SSO (Single Sign-On) project. It is built using .NET, providing authentication services and user management for the SSO ecosystem.

## Features

- .NET Web API project
- OAuth integration (Google, Plex)
- JWT authentication
- Entity Framework Core with SQL Server
- Session management
- CORS enabled for frontend
- Swagger for API documentation

## Getting Started

### Prerequisites

- .NET SDK 7.0+
- SQL Server (or adjust connection string as needed)

### Configuration

Configure environment variables and connection strings in `appsettings.json` and/or `.env` files. The app uses an environment variable or `"DefaultConnection"` string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SsoDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Development

```bash
dotnet restore
dotnet build
dotnet run
```

The API will start by default on `http://localhost:5000` or as specified in your launch settings.

### API Documentation

Swagger UI is available in development mode at `/swagger`.

## Project Structure

- `API/`: Controllers and routes
- `Infrastructure/`: Services (OAuth, JWT, etc.)
- `Models/`: Data models
- `Migrations/`: Database migrations
- `Program.cs`: Main entry point
- `appsettings.json`: Configuration

## License

[MIT](LICENSE)
