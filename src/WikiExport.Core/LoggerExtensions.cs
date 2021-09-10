using System;

using Microsoft.Extensions.Logging;

namespace WikiExport
{
    public static class LoggerExtensions
    {
        public static Exception LogRaiseException(this ILogger logger, string message, LogLevel level = LogLevel.Critical)
        {
            logger.Log(level, message);
            throw new Exception(message);
        }
    }
}