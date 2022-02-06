using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{

    public class TimeService
    {
        public string Format => _format;
        readonly string _format;

        //readonly DateTime _dateOfInception;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">Example using the Microsoft specs for DateTime ToString format: "yyyy-MM-dd HH:mm"</param>
        public TimeService(string format)
        {
            _format = format;
            //_dateOfInception = dateOfInception;
        }

        public string NowUTC()
        {
            return DateTime.UtcNow.ToString(_format);
        }

        /// <summary>
        /// Time service constructor requires a dateOfInception. This method returns the seconds since inception.
        /// </summary>
        /// <returns></returns>
        /*public uint NowUTC_SecondsSinceInception()
        {
             return (uint)DateTime.UtcNow.Subtract(_dateOfInception).TotalSeconds;
        }*/

        /// <summary>
        /// Time since 1970/1/1
        /// </summary>
        /// <returns></returns>
        public ulong NowUTC_AsUnixTime()
        {
            return (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
