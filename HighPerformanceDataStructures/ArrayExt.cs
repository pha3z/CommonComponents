using System;
using System.Collections.Generic;
using System.Text;

namespace Faeric.HighPerformanceDataStructures
{
    public static class ArrayExt
    {
        /// <summary>
        /// Searches the array to see if it contains item. If not, adds the item after the searchLen. Returns new searchLen (unchanged or incremented).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="itm"></param>
        /// <param name="searchLen"></param>
        /// <returns></returns>
        public static int AddIfNotPresent<T>(this T[] a, T itm, int searchLen)
        {
            for(int i = 0; i < searchLen; i++)
            {
                if(EqualityComparer<T>.Default.Equals(a[i], itm))
                    return searchLen;
            }

            a[searchLen + 1] = itm;
            return searchLen + 1;
        }
    }
}
