using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public static class LoggingStartupExtention
    {
        public static ILoggingBuilder AddCustomConsoleFormat(this ILoggingBuilder builder)
        {
            return builder.AddConsole(options =>
            {
                options.FormatterName = "custom";
            }).AddConsoleFormatter<CustomLogFormatter, ConsoleFormatterOptions>(options =>
                {
                    options.TimestampFormat = "dd.MM.yyyy HH:mm:ss";
                    options.UseUtcTimestamp = false;
                    options.IncludeScopes = true;
                });
        }

        public static ILoggingBuilder AddCustomConsoleFormat(this ILoggingBuilder builder, Action<ConsoleFormatterOptions> configure)
        {
            return builder.AddConsole(options =>
            {
                options.FormatterName = "custom";
            }).AddConsoleFormatter<CustomLogFormatter, ConsoleFormatterOptions>(configure);
        }
        
    }
}
