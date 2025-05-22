# Company Employees API + LoggingService

A minimal **ASP.NET Core Web API** (preview .NET 9) that demonstrates clean project organisation, custom structured logging through a separate **LoggingService** class-library, and straightforward CLI workflows.

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
10. [License](#license)

---

## Architecture
```
company-employees.sln
│
├─ CompanyEmployees.Api.csproj  ← ASP.NET Core Web API (SDK ➜ `Microsoft.NET.Sdk.Web`)
│   └─ Program.cs               ← Configures DI & minimal APIs / MVC controllers
│
└─ LoggingService
   └─ LoggingService.csproj     ← Class Library (SDK ➜ `Microsoft.NET.Sdk`)
       ├─ ILoggerManager.cs     ← Logging abstraction (Singleton)
       └─ LoggerManager.cs      ← Concrete implementation (wraps `ILogger<T>`)
```

* **Loose coupling:** API depends only on `ILoggerManager`, not on any logging provider.  
* **Single Responsibility:** All cross-cutting logging logic lives inside `LoggingService`.

---

## Prerequisites
| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | **9.0 preview 4** (or later) | `dotnet --version` |
| PowerShell or Bash | — | CLI commands below |
| Git | Latest | To clone the repository |


---

## Getting Started

```bash
git clone https://github.com/Alejandroq12/company-employees.git
cd company-employees
dotnet restore                   # Restores all projects in the solution
```

> **Tip:** always execute CLI commands at the **solution root** unless otherwise stated.

---

## Project Structure

```text
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
```

`CompanyEmployees.Api.csproj` contains:

```xml
<ItemGroup>
  <!-- Exclude LoggingService source files from this project -->
  <Compile Remove="LoggingService\**\*.cs" />

  <!-- Reference the LoggingService project -->
  <ProjectReference Include="LoggingService\LoggingService.csproj" />
</ItemGroup>
```

---

## Building & Running

```bash
# Clean & build the entire solution
dotnet clean company-employees.sln
dotnet build company-employees.sln

# Run the Web API (explicit project)
dotnet run --project CompanyEmployees.Api.csproj
```

Navigate to `https://localhost:5001/weatherforecast`

---

## LoggingService Integration

### 1. Library code (`LoggingService`)

```csharp
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
```

> `ILogger<LoggerManager>` is injected automatically by the host; no manual provider plumbing required.

### 2. Dependency Injection (`Program.cs`)

```csharp
builder.Services.AddSingleton<ILoggerManager, LoggerManager>();
```

That is **all** you need—`LoggerManager` becomes a singleton service available throughout the API.

---

## Usage Example

```csharp
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
        // Business logic…
        return Enumerable.Empty<WeatherForecast>();
    }
}
```

---

## CLI Cheat-Sheet

| Task | Command |
|------|---------|
| Restore all packages | `dotnet restore company-employees.sln` |
| Clean build output   | `dotnet clean company-employees.sln` |
| Build entire solution| `dotnet build company-employees.sln` |
| Run the API project  | `dotnet run --project CompanyEmployees.Api.csproj` |
| Add new project to solution | `dotnet sln add <path>/<NewLib>.csproj` |
| Reference a project  | `dotnet add <consumer>.csproj reference <lib>.csproj` |

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| **`MSB1011`** “specify a project … this folder contains more than one” | Always pass the solution or project file (`dotnet build company-employees.sln`). |
| **`Unable to resolve service for type 'Microsoft.Extensions.Logging.ILogger'`** | Ensure `LoggerManager` injects `ILogger<LoggerManager>` *not* plain `ILogger`; verify DI registration. |
| Duplicate assembly attributes (`CS0579`) | Confirm `<Compile Remove="LoggingService\**\*.cs" />` exists so API does not compile library source twice. |
| `NU1900` warnings for Telerik feed | Remove the feed: `dotnet nuget remove source telerik`. |

---

## License
MIT © 2025