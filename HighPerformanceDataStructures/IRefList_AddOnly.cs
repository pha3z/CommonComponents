using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public interface IRefList_AddOnly<T>
    {
        /// <summary>
        /// Same as AddByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.
        /// </summary>
        /// <returns></returns>
        ref T AddByRef();

        /// <summary>
        /// Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.
        /// </summary>
        /// <returns>A reference to the new item. You can mutate it as you please.</returns>
        ref T AddByRef_Unsafe();
        int AddInOrderedPosition(ref T item, RefGreaterThan<T> refGreaterThanTest);
        int AddInOrderedPosition_Unsafe(ref T item, RefGreaterThan<T> refGreaterThanTest);

        /// <summary>
        /// Makes sure array size is at least capacity. If not, size is increased to exactly capacity.
        /// </summary>
        /// <param name="capacity"></param>
        void EnsureCapacity(int capacity);
    }
}
