using Serilog.Events;
using Steel.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    [SerialType(Properties =Flags.Public, Fields =Flags.Public)]
    public class LogConfig
    {
        public string fileOutputPath;
        public bool consoleLog;
        public LogEventLevel consoleLevel;
        public bool fileLog;
        public LogEventLevel fileLevel;

    }
}
