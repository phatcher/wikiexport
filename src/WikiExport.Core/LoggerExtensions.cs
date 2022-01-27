using Microsoft.Extensions.Logging;

namespace WikiExport
{
    public static class LoggerExtensions
    {
        public static void LogException(this ILogger logger, LogLevel level, string message, ExportOptions options)
        {
            logger.Log(level, message);
            if (level >= options.FatalErrorLevel)
            {
                options.ErrorMessages.Add(message);
            }
        }
    }
}