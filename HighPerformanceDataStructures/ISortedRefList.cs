using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public interface ISortedRefList<T> : IRefList_AddOnly<T>
    {
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

        void Clear();
        void InsertionSort(RefGreaterThan<T> greaterThan);
        void QuickSort(RefGreaterThan<T> comparer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        int RemoveFirstMatch_RetainingOrder(RefPredicate<T> predicate);
        void RemoveFirstN(int n);
        void RemoveLast();
        void RemoveLastN(int n);
        void Remove_RetainingOrder(int idx);
    }
}
