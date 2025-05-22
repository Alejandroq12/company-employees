using System.IO.Compression;
using Microsoft.Extensions.Logging;
namespace LoggingService;

public class LoggerManager(ILogger<LoggerManager> logger) : ILoggerManager
{
    public void LogDebug(string message) => logger.LogDebug(message);
    public void LogInformation(string message) => logger.LogInformation(message);
    public void LogWarning(string message) => logger.LogWarning(message);
    public void LogError(string message) => logger.LogError(message);
}
