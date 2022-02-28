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

        static Regex _angleRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Uses a char array loop, which is 10x faster than a simple regex.<br/>
        /// This method is unaltered straight from DotNetPerls. It should be 100% perfect.
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
                char c = src[i];
                if (c == '<')
                {
                    inside = true;
                    continue;
                }
                if (c == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = c;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        /// <summary>
        /// Uses a char array loop, which is 10x faster than a simple regex.<br/>
        /// This is James' own method. Not guaranteed to be bugfree.
        /// </summary>
        /// <param name="tagLengthLimit">If the tag exceeds this limit, it will not be stripped. Length includes the open and close brackets.</param>
        /// <returns></returns>
        public static string StripAngleTags(string src, int tagLengthLimit)
        {
            if (tagLengthLimit < 3)
                throw new ArgumentOutOfRangeException(nameof(tagLengthLimit), "Must be greater than 2");

            char[] array = new char[src.Length];
            int arrayIndex = 0;

            for (int i = 0; i < src.Length; i++)
            {
                char c = src[i];
                if (c == '<')
                {
                    //Locate closing bracket
                    int tagEndBracket = -1;
                    for(int j = i + 1; j < src.Length; j++)
                    {
                        if (src[j] == '>')
                        {
                            tagEndBracket = j;
                            break;
                        }
                        
                        if (j - i > tagLengthLimit + 1)
                            break;
                    }

                    //If close bracket found, skip ahead to it.
                    //Otherwise, copy the opening bracket into output array and continue forward.
                    if (tagEndBracket != -1)
                    {
                        i = tagEndBracket;
                        continue;
                    }
                }

                array[arrayIndex] = c;
                arrayIndex++;
                
            }
            return new string(array, 0, arrayIndex);
        }

        public static string StripParagraphTags(string src)
        {
            var parts = src.Split(new string[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);
            return String.Concat(parts);
        }
    }
    
}
