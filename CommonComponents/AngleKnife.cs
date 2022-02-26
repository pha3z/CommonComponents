using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    /// <summary>
    /// A class for minimal HTML,SVG, and XML processing.<br/>
    /// For anything sophisticated, use the amazing library AngleSharp instead.<br/>
    /// </summary>
    public static class AngleKnife
    {

        //static Regex _angleRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Uses a char array loop, which is 10x faster than a simple regex.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string StripAngleTags(string src)
        {
            //return _angleRegex.Replace(src, string.Empty);

            char[] array = new char[src.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < src.Length; i++)
            {
                char let = src[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
    
}
