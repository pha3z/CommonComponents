using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ExceptionExtensions
    {
        //public static string 
        /// <summary>
        /// Collects all inner exceptions together into a single string. Handles all exception types, including AggregateException.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="LineDelimited"></param>
        /// <returns></returns>
        public static string AllMessages(this Exception ex, bool LineDelimited = false)
        {
            string msg = "";

            if (ex.GetType() == typeof(AggregateException))
            {
                var agEx = ex as AggregateException;

                foreach (var e in agEx.InnerExceptions)
                {
                    if (LineDelimited)
                        msg += "\n";

                    msg += e.Message;
                }

                return msg;
            }

            if (ex.InnerException == null)
                return ex.Message;
            else
            {
                msg = ex.Message;

                if (LineDelimited)
                    msg += "\n";

                return msg += " --> " + ex.InnerException.AllMessages(LineDelimited);
            }
        }

        //public static string 
        /// <summary>
        /// Gets stack trace of first exception and ALL inner exceptions. Also traces all inner exceptions for AggregateException.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FullStackTrace(this Exception ex)
        {
            string msg = "";

            if (ex.GetType() == typeof(AggregateException))
            {
                var agEx = ex as AggregateException;

                foreach (var e in agEx.InnerExceptions)
                    msg += "\n \n   INNER TRACE: " + ex.InnerException.FullStackTrace();

                return msg;
            }

            if (ex.InnerException == null)
                return ex.Message;
            else
            {
                msg = ex.StackTrace;

                return msg += "\n \n   INNER TRACE: " + ex.InnerException.FullStackTrace();
            }
        }
    }
}
