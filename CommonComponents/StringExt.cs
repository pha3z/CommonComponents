using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;

namespace Common
{
    public static class StringExt
    {
        public static string ToSafeString(this int? number, string format = null)
        {
            if (number.HasValue)
            {
                if(format == null)
                    return number.Value.ToString();
                else
                    return number.Value.ToString(format);
            }
            else
                return "";
        }

        public static string ToSafeString(this long? number, string format = null)
        {
            if (number.HasValue)
            {
                if (format == null)
                    return number.Value.ToString();
                else
                    return number.Value.ToString(format);
            }
            else
                return "";
        }

        public static string Truncate(this string s, int maxLen)
        {
            return s.Substring(0, Min(maxLen, s.Length));
        }

        /// Returns everything before suffix.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string RemoveSuffix(this string s, string suffix, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                var sLower = s.ToLower();
                suffix = suffix.ToLower();

                if (!sLower.EndsWith(suffix))
                    return s;

                int i = sLower.LastIndexOf(suffix);
                return s.Substring(0, i);
            }
            else
            {
                if (!s.EndsWith(suffix))
                    return s;

                int i = s.LastIndexOf(suffix);
                return s.Substring(0, i);
            }
        }

        public static bool IsNullOrEmpty(this string s) => (s == null || s.Length < 1);
        public static bool IsNullOrWhitespace(this string s) => (s == null || s.Trim().Length < 1);
    }
}
