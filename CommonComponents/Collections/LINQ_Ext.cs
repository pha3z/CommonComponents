using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Common.Collections
{
    public static class LINQ_Ext
    {
        public static bool IsEmpty<T>(this IEnumerable<T> c) => !c.Any();

        /// <summary>
        /// Does Not Contain
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Lacks<T>(this IEnumerable<T> c, T value)
        {
            return !c.Contains(value);
        }

        public static IEnumerable<string> Except(this IEnumerable<string> c, string value)
        {
            return c.Except(new string[] { value });
        }

        /// <summary>
        /// If you need to reference the index of the element in the sequence, pass an action that takes an int as second parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="actn"></param>
        public static void Each<T>(this IEnumerable<T> c, Action<T> actn)
        {
            foreach (T el in c)
                actn(el);
        }

        /// <summary>
        /// Applies an action to every element of the sequence where the second argument of action is the index in the sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="actn"></param>
        public static void Each<T>(this IEnumerable<T> c, Action<T, int> actn)
        {
            int i = 0;
            foreach (T el in c)
            {
                actn(el, i);
                i++;
            }
        }

        /// <summary>
        /// Creates a new collection of tuples where the singleValue combined with every element of the source collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="src"></param>
        /// <param name="singleValue"></param>
        /// <param name="putSingleValueFirst">If true, each tuple will be of pattern (singleValue, srcElement). If false, then (srcElement, singleValue)</param>
        /// <returns></returns>
        public static IEnumerable<(T,V)> ZipWithOne<T,V>(this IEnumerable<T> src, V singleValue, bool putSingleValueFirst = false)
        {
            foreach (var element in src)
                yield return (element, singleValue);
        }
    }
}
