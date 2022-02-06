using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Collections
{
    //Possible? These could possibly operate on IList instead of Array, which would make them applicable to List as well.
    public static class ArrayExtn
    {

        public static int FindIndexOf<T>(this T[] a, int start, Predicate<T> predicate)
        {
            for (int i = start; i < a.Length; i++)
            {
                if (predicate(a[i]))
                    return i;
            }

            return -1;
        }
    }
}
