using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common
{
    public static class LogFactory
    {
        /// <summary>
        /// In present configuration, console and file logging is always enabled.
        /// </summary>
        /// <param name="config"></param>
        public static Logger CreateLogger(LogConfig config)
        {
            Console.WriteLine($"Using log file path: {config.fileOutputPath}");

            string folder = Path.GetDirectoryName(config.fileOutputPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string template = "{Timestamp:MMM dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug();

            if (config.consoleLog)
                logConfig = logConfig
                    .WriteTo.Console(restrictedToMinimumLevel: config.consoleLevel, outputTemplate: template);

            if (config.fileLog)
            {
                logConfig = logConfig
                .WriteTo.File(
                    restrictedToMinimumLevel: config.fileLevel,
                    path: config.fileOutputPath,
                    outputTemplate: template,
                    flushToDiskInterval: TimeSpan.FromMinutes(1),
                    rollingInterval: RollingInterval.Month,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 2000000, //2 MB
                    retainedFileCountLimit: 31
                    );
            }

            return logConfig.CreateLogger();
        }


        /// <summary>
        /// Fast way to create simple console logging with fewer config requirements.
        /// </summary>
        /// <param name="level"></param>
        public static Logger CreateLogger_ConsoleOnly(LogEventLevel level)
        {
            string template = "{Timestamp:MMM dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: level, outputTemplate: template)
                .CreateLogger();
        }
    }
}
