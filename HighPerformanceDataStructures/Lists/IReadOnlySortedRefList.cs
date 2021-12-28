using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public interface IReadOnlySortedRefList<T>
    {
        int Count { get; }

        /// <summary>
        /// CAUTION: If you mutate the item directly, make sure you do not change a field that is used in sorting, unless you intend to manually invoke a Sort() operation immediately to resort.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        ref T this[int idx] { get; }

        /// <summary>
        /// CAUTION: If you mutate the item directly, make sure you do not change a field that is used in sorting, unless you intend to manually invoke a Sort() operation immediately to resort.
        /// </summary>
        ref T Last { get; }
    }
}
