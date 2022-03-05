using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public interface IReadOnlyRefList<T>
    {
        int Count { get; }

        /// <summary>
        /// CAUTION: Do not inadvertently mutate the item. The purpose of the ref is to avoid copying data.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        ref T this[int idx] { get; }

        /// <summary>
        /// CAUTION: Do not inadvertently mutate the item. The purpose of the ref is to avoid copying data.
        /// </summary>
        ref T Last { get; }
    }
}
