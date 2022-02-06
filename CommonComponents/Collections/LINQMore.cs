using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Collections
{
    /// <summary>
    /// An excerpt of some of the extensions found here https://github.com/morelinq/MoreLINQ
    /// Avoids a dependency on the library and bringing in a lot of unnecessary stuff
    /// 
    /// </summary>
    public static class LINQMore
    {
        /// <summary>
        /// NOTE: Consider GroupBy Before this method if you're doing anything more complex than simply getting distinct elements
        /// GroupBy is much more flexible. The only reason to use DistinctBy is when you ONLY care about grabbing the first distinct
        /// element encountered for each group
        /// GroupBy involves much more processing and memory but you can do a lot more sophisticate stuff
        /// such as Selecting particular elements on group values after groups are created.
        /// https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the default equality comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }

        /// <summary>
        /// NOTE: Consider GroupBy Before this method if you're doing anything more complex than simply getting distinct elements
        /// GroupBy is much more flexible. The only reason to use DistinctBy is when you ONLY care about grabbing the first distinct
        /// element encountered for each group
        /// GroupBy involves much more processing and memory but you can do a lot more sophisticate stuff
        /// such as Selecting particular elements on group values after groups are created.
        /// https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the specified comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal.
        /// If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            if (!source.Any())
                return source;

            return _(); IEnumerable<TSource> _()
            {
                //JAMES HOUX 11/22/220
                //Original method signature of MoreLinq passes an IEqualityComparer<TKey>? comparer
                //The question mark indicates a reference type and is only compatiable with Standard 2.1 or greater
                //i rewrote the next few lines of code to handle a null comparer
                HashSet<TKey> knownKeys;

                if(comparer != null)
                    knownKeys = new HashSet<TKey>(comparer);
                else
                    knownKeys = new HashSet<TKey>();

                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
    }
}
