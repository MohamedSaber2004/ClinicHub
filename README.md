# ClinicHub

A modern, scalable ASP.NET Core-based healthcare clinic management system built with clean architecture principles and comprehensive health monitoring.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Health Checks](#health-checks)
- [Logging](#logging)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

ClinicHub is a comprehensive healthcare management platform designed to streamline clinic operations. It provides a robust API for managing clinic workflows, patient information, and clinical processes with built-in health monitoring and comprehensive logging capabilities.

Built with **C# 14.0** and **.NET 10**, ClinicHub leverages the latest Microsoft technologies to deliver high-performance, maintainable, and scalable healthcare solutions.

## ✨ Features

- **RESTful API**: Fully-featured REST API with OpenAPI/Swagger documentation
- **Health Monitoring**: Comprehensive health checks including:
  - Custom API health checks
  - SQL Server database connectivity
  - Disk storage monitoring
- **Structured Logging**: Enterprise-grade logging with Serilog
- **Environment Management**: Multi-environment configuration support (Development, Live, Production)
- **User Secrets**: Secure credential management for sensitive data
- **HTTPS Security**: Built-in HTTPS redirection
- **Authorization**: Authentication and authorization middleware
- **Interactive API Documentation**: Scalar API Reference UI

## 🛠 Tech Stack

### Core Framework
- **.NET 10** - Latest .NET runtime
- **C# 14.0** - Modern C# language features
- **ASP.NET Core** - Web framework

### Libraries & Tools
- **OpenAPI** - API specification and documentation
- **Scalar.AspNetCore** - Interactive API reference UI
- **Serilog** - Structured logging framework
- **HealthChecks** - AspNetCore health check system
  - `AspNetCore.HealthChecks.SqlServer` - Database health checks
  - `AspNetCore.HealthChecks.System` - System health checks (disk, memory)
- **Entity Framework Core** - ORM (via Persistence layer)

### Development Environment
- **Visual Studio 2026** (Community Edition)
- **PowerShell** - Task automation and scripting

## 📁 Project Structure

The solution follows **Clean Architecture** principles with the following layers:

```
ClinicHub/
├── ClinicHub.API/                    # Presentation Layer
│   ├── Program.cs                    # Application entry point & configuration
│   ├── Controllers/                  # API endpoints
│   ├── appsettings.json             # Configuration files
│   └── appsettings.{env}.json       # Environment-specific settings
│
├── ClinicHub.Application/            # Application/Business Logic Layer
│   ├── HealthCheck/                  # Health check implementations
│   │   ├── CustomHealthCheck.cs     # Custom health check logic
│   │   └── HealthCheckResponseWriter.cs
│   ├── DependencyInjection.cs       # Service registration
│   └── Services/                     # Application services
│
├── ClinicHub.Domain/                 # Domain/Core Layer
│   ├── Entities/                     # Domain models
│   ├── Interfaces/                   # Domain abstractions
│   └── Exceptions/                   # Domain exceptions
│
├── ClinicHub.Infrastructure/         # Infrastructure Layer
│   ├── ExternalServices/            # Third-party integrations
│   └── Utilities/                    # Infrastructure utilities
│
├── ClinicHub.Persistence/           # Data Access Layer
│   ├── DbContext/                   # Entity Framework context
│   ├── Repositories/                # Data access repositories
│   └── Migrations/                  # Database migrations
│
└── README.md                         # This file
```

### Layer Responsibilities

- **API Layer**: Handles HTTP requests, routing, and response formatting
- **Application Layer**: Contains business logic, services, and use cases
- **Domain Layer**: Defines core business entities and domain rules
- **Infrastructure Layer**: Manages external services and dependencies
- **Persistence Layer**: Handles database operations and data access

## 📋 Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server** (2019 or later)
  - Local instance or remote connection
  - Database connection credentials
- **Visual Studio 2022+** or **Visual Studio Code** (optional)
- **Git** - For version control

## 🚀 Getting Started

### 1. Clone the Repository

```powershell
git clone https://github.com/MohamedSaber2004/ClinicHub.git
cd ClinicHub
```

### 2. Restore NuGet Packages

```powershell
dotnet restore
```

### 3. Configure Database Connection

Edit `appsettings.json` (or environment-specific settings) with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "CareClinicHubDb": "Server=YOUR_SERVER;Database=CareClinicHub;User Id=sa;Password=YOUR_PASSWORD;"
  }
}
```

### 4. Apply Database Migrations

```powershell
dotnet ef database update --project ClinicHub.Persistence
```

### 5. Build the Solution

```powershell
dotnet build
```

## ⚙️ Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "CareClinicHubDb": "Server=localhost;Database=CareClinicHub;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment-Specific Configuration

The application supports environment-specific configuration files:

- `appsettings.json` - Shared configuration
- `appsettings.Development.json` - Development environment settings
- `appsettings.Live.json` - Live/staging environment settings
- `appsettings.Production.json` - Production environment settings

### User Secrets (Development & Live)

For sensitive data in Development and Live environments:

```powershell
# Initialize user secrets
dotnet user-secrets init --project ClinicHub.API

# Set connection string
dotnet user-secrets set "ConnectionStrings:CareClinicHubDb" "your_connection_string" --project ClinicHub.API
```

### Environment Variables

The application reads environment variables automatically:

```powershell
# Set via PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Or permanently
[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "User")
```

## 🏃 Running the Application

### Using .NET CLI

```powershell
dotnet run --project ClinicHub.API
```

### Using Visual Studio

1. Set `ClinicHub.API` as the startup project
2. Press `F5` or click **Run**
3. The application starts on `https://localhost:7000` (or assigned port)

### Development with Hot Reload

```powershell
dotnet watch run --project ClinicHub.API
```

## 📚 API Documentation

### Accessing the API

- **Base URL**: `https://localhost:7000`
- **API Version**: V1
- **Protocol**: HTTPS (HTTP redirects to HTTPS)

### Interactive Documentation

**Development Environment Only**

- **Scalar UI**: `https://localhost:7000/scalar`
- **OpenAPI Spec**: `https://localhost:7000/openapi/v1.json`

The Scalar UI provides:
- Interactive endpoint testing
- Request/response examples
- Parameter documentation
- Authorization testing

### Example API Calls

```bash
# Health check
curl https://localhost:7000/health

# API endpoints (examples)
curl https://localhost:7000/api/clinics
curl https://localhost:7000/api/patients
```

## 🏥 Health Checks

The application includes comprehensive health monitoring accessible at `/health`.

### Health Check Components

1. **Custom Health Check** - API-specific health verification
2. **Database Health Check** - SQL Server connectivity and responsiveness
3. **Disk Storage Health Check** - System disk space monitoring (minimum 1GB)

### Health Check Response

```json
{
  "status": "Healthy",
  "checks": {
    "API Custom Checks": {
      "status": "Healthy"
    },
    "Disk Space": {
      "status": "Healthy"
    },
    "Database": {
      "status": "Healthy",
      "description": "Connection successful"
    }
  }
}
```

### Monitoring Health Checks

```bash
# Check application health
curl https://localhost:7000/health

# Use in monitoring tools (Prometheus, DataDog, etc.)
# The health check endpoint returns standard HTTP status codes
# 200 OK - All checks healthy
# 503 Service Unavailable - One or more checks failed
```

## 📝 Logging

Serilog is configured for structured, enterprise-grade logging.

### Log Levels

- **Information** - General application flow (default)
- **Warning** - Potentially harmful situations
- **Error** - Error events that still allow operation continuation
- **Fatal** - Severe errors causing application termination
- **Debug** - Diagnostic information

### Log Output

Logs are written to:

1. **Console** - Real-time development feedback
2. **File** - Persistent logging (configured in Serilog settings)

### Accessing Logs

```powershell
# View application logs
Get-Content logs/clinichub-.txt | Select-Object -Last 100

# Filter by severity
Get-Content logs/clinichub-.txt | Select-String "Error|Fatal"
```

### Log Configuration

Edit `appsettings.json` to adjust logging:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/clinichub-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 👨‍💻 Development

### Project Development Workflow

1. **Create a Feature Branch**
   ```powershell
   git checkout -b feature/your-feature-name
   ```

2. **Implement Changes**
   - Follow the Clean Architecture pattern
   - Keep concerns separated across layers
   - Add appropriate logging and error handling

3. **Build and Test**
   ```powershell
   dotnet build
   dotnet test
   ```

4. **Verify Health Checks**
   ```powershell
   curl https://localhost:7000/health
   ```

5. **Commit and Push**
   ```powershell
   git add .
   git commit -m "feature: add your feature description"
   git push origin feature/your-feature-name
   ```

### Code Guidelines

- **Architecture**: Maintain Clean Architecture separation
- **Naming**: Use PascalCase for classes and methods, camelCase for variables
- **Comments**: Add XML documentation for public APIs
- **Async**: Use async/await for I/O operations
- **Error Handling**: Use specific exceptions and proper logging
- **Testing**: Write unit and integration tests for new features

### Adding New Endpoints

1. Create controller in `ClinicHub.API/Controllers/`
2. Define domain entities in `ClinicHub.Domain/Entities/`
3. Create services in `ClinicHub.Application/Services/`
4. Implement repositories in `ClinicHub.Persistence/Repositories/`

### Database Migrations

```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project ClinicHub.Persistence

# Apply migrations
dotnet ef database update --project ClinicHub.Persistence

# Remove last migration
dotnet ef migrations remove --project ClinicHub.Persistence
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the Repository** on GitHub
2. **Create a Feature Branch**
   ```powershell
   git checkout -b feature/your-feature
   ```
3. **Commit Your Changes**
   ```powershell
   git commit -m "feature: description of changes"
   ```
4. **Push to Your Fork**
   ```powershell
   git push origin feature/your-feature
   ```
5. **Create a Pull Request**
   - Provide a clear description of changes
   - Reference any related issues
   - Ensure all tests pass
   - Include any breaking changes clearly

### Development Standards

- Follow Microsoft C# coding conventions
- Maintain backward compatibility where possible
- Add tests for new features
- Update documentation
- Use meaningful commit messages

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

## 📞 Support

For issues, questions, or suggestions:

1. **GitHub Issues**: [Open an Issue](https://github.com/MohamedSaber2004/ClinicHub/issues)
2. **GitHub Discussions**: [Start a Discussion](https://github.com/MohamedSaber2004/ClinicHub/discussions)
3. **Documentation**: Check existing documentation and code comments

## 🔗 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Serilog Documentation](https://github.com/serilog/serilog/wiki)
- [OpenAPI Specification](https://www.openapis.org/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Last Updated**: 2024
**Maintained By**: ClinicHub Development Team
**Repository**: https://github.com/MohamedSaber2004/ClinicHub