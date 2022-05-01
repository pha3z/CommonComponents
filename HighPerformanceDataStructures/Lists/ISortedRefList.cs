using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public interface ISortedRefList<T> : IReadOnlySortedRefList<T>, IReadOnlyRefList<T>, IRefList_AddOnly<T>
    {
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
