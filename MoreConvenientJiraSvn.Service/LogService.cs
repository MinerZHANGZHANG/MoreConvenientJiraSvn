using Microsoft.Extensions.Logging;

namespace MoreConvenientJiraSvn.Service
{
    public class LogService(ILoggerFactory loggerFactory, bool isDebugMode = true) : IDisposable
    {
        private readonly bool _isDebugMode = isDebugMode;
        private readonly ILogger _logger = loggerFactory.CreateLogger<LogService>();

        public void LogDebug(string message)
        {
            if (_isDebugMode)
            {
                Console.WriteLine($"DEBUG: {message}");
            }
            _logger.LogDebug(message);
        }

        public void LogInfo(string message)
        {
            if (_isDebugMode)
            {
                Console.WriteLine($"INFO: {message}");
            }
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            if (_isDebugMode)
            {
                Console.WriteLine($"WARNING: {message}");
            }
            _logger.LogWarning(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            if (_isDebugMode)
            {
                Console.WriteLine($"ERROR: {message}");
            }
            if (exception != null)
            {
                _logger.LogError(exception, message);
            }
            else
            {
                _logger.LogError(message);
            }
        }

        public void Dispose()
        {
            LogInfo("Application close.");
            loggerFactory.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
