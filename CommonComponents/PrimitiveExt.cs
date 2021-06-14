using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class PrimitiveExt
    {
        /// <summary>
        /// Prints true or false in all lower case.  The built-in NET ToString() method capitalizes first letter
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string ToLower(this bool b) => b ? "true" : "false";
    }
}
