using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Text;

namespace Logging
{
    public sealed class CustomLogFormatter : ConsoleFormatter
    {
        private const string DefaultTimestampFormat = "dd.MM.yyyy HH:mm:ss";
        private readonly bool _useUtcTimestamp;
        private readonly string _timestampFormat;

        public CustomLogFormatter(IOptionsMonitor<ConsoleFormatterOptions> options)
            : base("custom")
        {
            _timestampFormat = options.CurrentValue.TimestampFormat ?? DefaultTimestampFormat;
            _useUtcTimestamp = options.CurrentValue.UseUtcTimestamp;
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            var timestamp = _useUtcTimestamp
                ? DateTime.UtcNow.ToString(_timestampFormat)
                : DateTime.Now.ToString(_timestampFormat);

            var logLevel = GetLogLevelString(logEntry.LogLevel);

            // Исправленное извлечение имени класса
            var lastDotIndex = logEntry.Category.LastIndexOf('.');
            var category = lastDotIndex >= 0
                ? logEntry.Category.Substring(lastDotIndex + 1)
                : logEntry.Category;

            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

            var sb = new StringBuilder();
            sb.Append(timestamp)
              .Append(' ')
              .Append(logLevel)
              .Append(" [")
              .Append(category)
              .Append("] ")
              .Append(message);

            if (logEntry.Exception != null)
            {
                sb.AppendLine().Append(logEntry.Exception);
            }

            textWriter.WriteLine(sb.ToString());
        }

        private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}
