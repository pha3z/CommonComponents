using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Collections.NonGeneric
{
    public static partial class IEnumerable_NonGeneric_Ext
    {
        public static List<object> ToList(this IEnumerable source)
        {
            var list = new List<object>(source.Count());
            foreach (var o in source)
                list.Add(o);

            return list;
        }

        public static IEnumerable Select(this IEnumerable source, Func<object, object> pred)
        {
            foreach (object o in source)
                yield return pred(o);

        }

        public static IEnumerable<T> Select<T>(this IEnumerable source, Func<object, object> pred)
        {
            foreach (object o in source)
                yield return (T)pred(o);

        }

        public static bool Any(this IEnumerable source, Predicate<object> pred = null)
        {
            if (pred == null)
                return FirstOrNull(source) != null;
            else
                return FirstOrNull(source, pred) != null;
        }

        public static object FirstOrNull(this IEnumerable source, Predicate<object> pred)
        {
            foreach (object o in source)
            {
                if (pred(o))
                {
                    return o;
                }
            }

            return null;
        }
        public static object FirstOrNull(this IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            if (enumerator.MoveNext()) return enumerator.Current;
            return null;
        }

        public static object First(this IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static int Count(this IEnumerable source)
        {
            if (source is ICollection countable)
            {
                return countable.Count;
            }
            else
            {
                int result = 0;

                foreach (object _ in source)
                {
                    result++;
                }

                return result;
            }
        }
    }
}