# Company Employees API + LoggingService

A minimal **ASP.NET Core Web API** (preview .NET 9) that demonstrates clean project organisation, custom structured logging through an external **LoggingService** class-library, and straightforward CLI workflows.

---

## Table of Contents
1. [Architecture](#architecture)
2. [Prerequisites](#prerequisites)
3. [Getting Started](#getting-started)
4. [Project Structure](#project-structure)
5. [Building & Running](#building--running)
6. [LoggingService Integration](#loggingservice-integration)
7. [Usage Example](#usage-example)
8. [CLI Cheat-Sheet](#cli-cheat-sheet)
9. [Troubleshooting](#troubleshooting)
10. [EF Core Migrations (LocalDB)](#ef-core-migrations-localdb)
11. [License](#license)

---

## Architecture
    company-employees.sln
    │
    ├─ CompanyEmployees.Api.csproj   ← ASP.NET Core Web API (SDK → Microsoft.NET.Sdk.Web)
    │   └─ Program.cs                ← Configures DI & minimal APIs / MVC controllers
    │
    └─ LoggingService
        └─ LoggingService.csproj     ← Class Library (SDK → Microsoft.NET.Sdk)
            ├─ ILoggerManager.cs     ← Logging abstraction (Singleton)
            └─ LoggerManager.cs      ← Concrete implementation (wraps ILogger<T>)

* **Loose coupling :** API depends only on `ILoggerManager`, not on any concrete logging provider.  
* **Single Responsibility :** All cross-cutting logging logic lives inside *LoggingService*.

---

## Prerequisites

| Tool              | Version                        | Notes                               |
|-------------------|--------------------------------|-------------------------------------|
| .NET SDK          | **9.0 preview 4** (or later)   | `dotnet --version`                  |
| PowerShell / Bash | —                              | To run the CLI commands             |
| Git               | Latest                         | To clone the repository             |

---

## Getting Started

Clone the repo and restore packages:

    git clone https://github.com/Alejandroq12/company-employees.git
    cd company-employees
    dotnet restore

> Always run CLI commands from the **solution root** unless stated otherwise.

---

## Project Structure
    company-employees/
    │
    ├─ company-employees.sln
    ├─ CompanyEmployees.Api.csproj
    │   ├─ Controllers/
    │   ├─ Extensions/
    │   └─ Program.cs
    │
    └─ LoggingService/
        ├─ LoggingService.csproj
        ├─ ILoggerManager.cs
        └─ LoggerManager.cs

`CompanyEmployees.Api.csproj` excerpt:

    <ItemGroup>
      <!-- Exclude library source so it isn't compiled twice -->
      <Compile Remove="LoggingService\**\*.cs" />
      <ProjectReference Include="LoggingService\LoggingService.csproj" />
    </ItemGroup>

---

## Building & Running

    dotnet clean  company-employees.sln     # optional
    dotnet build  company-employees.sln
    dotnet run   --project CompanyEmployees.Api.csproj

Browse to **https://localhost:5001/weatherforecast**

---

## LoggingService Integration

### Library code

    // ILoggerManager.cs
    namespace LoggingService;
    public interface ILoggerManager
    {
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
    }

    // LoggerManager.cs
    using Microsoft.Extensions.Logging;

    namespace LoggingService;
    public sealed class LoggerManager(ILogger<LoggerManager> logger) : ILoggerManager
    {
        public void LogDebug(string message)        => logger.LogDebug(message);
        public void LogInformation(string message)  => logger.LogInformation(message);
        public void LogWarning(string message)      => logger.LogWarning(message);
        public void LogError(string message)        => logger.LogError(message);
    }

### Dependency Injection (Program.cs)

    builder.Services.AddSingleton<ILoggerManager, LoggerManager>();

---

## Usage Example

    using LoggingService;
    using Microsoft.AspNetCore.Mvc;

    namespace CompanyEmployees.Api.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILoggerManager logger) : ControllerBase
    {
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            logger.LogInformation("Fetching weather forecast.");
            return Enumerable.Empty<WeatherForecast>();
        }
    }

---

## CLI Cheat-Sheet

| Task                              | Command                                                            |
|-----------------------------------|--------------------------------------------------------------------|
| Restore packages                  | `dotnet restore company-employees.sln`                             |
| Clean build output                | `dotnet clean  company-employees.sln`                              |
| Build entire solution             | `dotnet build company-employees.sln`                               |
| Run the API                       | `dotnet run --project CompanyEmployees.Api.csproj`                 |
| Add new project to solution       | `dotnet sln add <path>/<NewLib>.csproj`                            |
| Reference another project         | `dotnet add <consumer>.csproj reference <lib>.csproj`              |

---

## Troubleshooting

| Symptom                                                     | Remedy                                                                                   |
|-------------------------------------------------------------|------------------------------------------------------------------------------------------|
| **MSB1011** “specify a project … this folder contains more than one” | Provide the solution or explicit project file to the command.                            |
| Cannot resolve `Microsoft.Extensions.Logging.ILogger`       | Ensure `LoggerManager` injects `ILogger<LoggerManager>` and is registered in DI.         |
| Duplicate assembly attributes (`CS0579`)                    | Keep the `<Compile Remove="LoggingService\**\*.cs" />` exclusion in the API project.     |
| `NU1900` Telerik feed warnings                              | Remove the feed: `dotnet nuget remove source telerik`                                    |

---

## EF Core Migrations (LocalDB)

Follow these four steps to spin up a fresh database on **LocalDB**.

1. **Discover the instance**

       sqllocaldb i          # Expected output: MSSQLLocalDB

2. **Start the instance**

       sqllocaldb start MSSQLLocalDB

3. **Create the initial migration**

       dotnet ef migrations add DatabaseCreation `
           --context RepositoryContext `
           --project ".\CompanyEmployees.Infrastructure.Persistence" `
           --startup-project "." `
           --output-dir Migrations

4. **Apply the migration**

       dotnet ef database update `
           --project ".\CompanyEmployees.Infrastructure.Persistence" `
           --startup-project "."

A database named **CompanyEmployeesDb** (or the name specified in *appsettings.json*) will now appear under **(localdb)\\MSSQLLocalDB** with all tables and `__EFMigrationsHistory` populated.

---

## License
MIT © 2025
