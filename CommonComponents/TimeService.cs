using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{

    public class TimeService
    {
        public string Format => _format;
        readonly string _format;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">Example using the Microsoft specs for DateTime ToString format: "yyyy-MM-dd HH:mm"</param>
        public TimeService(string format)
        {
            _format = format;
        }

        public string NowUTC()
        {
            return DateTime.UtcNow.ToString(_format);
        }
    }
}
