using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{

    /// <summary>
    /// Like FastList but for ValueTypes managed BY REF. Do NOT use this with Reference types -- use FastList for that purpose.
    /// <br/><br/>However, items are added strictly with AddByRef(), which eliminates unnecessary copies. This is useful for storing structs significantly larger than 4 bytes. There is still a 'ref' copy cost, which equates to a 4 byte (or 8-byte in 64-bit mode) copy anyway. So if your structs are less than 9 to 16 bytes and/or you want value-copy semantics, you probably should not use this data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RefList<T> : ISortedRefList<T>
    {
        T[] _items;

        public int Count => _count;
        int _count;

        public ref T this[int idx] => ref _items[idx];

        public ref T Last => ref _items[_count - 1];

        /// <summary>Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.</summary>
        public ref T AddByRef_Unsafe() => ref _items[_count++];

        /// <summary>Same as AddByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        public ref T AddByRef()
        {
            if (_items.Length == _count)
                IncreaseCapacity(_items.Length * 2);

            return ref _items[_count++];
        }

        /// <summary>Makes sure array size is at least capacity. If not, size is increased to exactly capacity.</summary>
        public void EnsureCapacity(int capacity)
        {
            if (_items.Length < capacity)
                IncreaseCapacity(capacity);
        }

        void IncreaseCapacity(int newCapacity)
        {
            var newArray = new T[newCapacity];
            Array.Copy(_items, newArray, _items.Length);
            _items = newArray; //Let GC handle the old items array
        }

        private RefList() { }

        public RefList(int capacity)
        {
            _items = new T[capacity];
        }

        /// <summary>Assumes list is already ordered according to the given refGreaterThanTest. Finds the correct position for new item and inserts it. Avg time: O(n/2)</summary>
        /// <param name="item"></param>
        /// <param name="refGreaterThanTest"></param>
        /// <returns>The index where the item was added.</returns>
        public int AddInOrderedPosition(ref T item, RefGreaterThan<T> refGreaterThanTest)
        {
            if (Count > _items.Length)
                IncreaseCapacity(Count * 1);

            return AddInOrderedPosition_Unsafe(ref item, refGreaterThanTest);
        }

        /// <summary>Assumes list is already ordered according to the given refGreaterThanTest. Finds the correct position for new item and inserts it. Avg time: O(n/2)</summary>
        /// <param name="item"></param>
        /// <param name="refGreaterThanTest"></param>
        /// <returns>The index where the item was added.</returns>
        public int AddInOrderedPosition_Unsafe(ref T item, RefGreaterThan<T> refGreaterThanTest)
        {
            int i = 0;
            for (; i < Count; i++)
            {
                if(refGreaterThanTest(ref _items[i], ref item))
                {
                    //Open a hole by shifting everything to the right
                    _count++;
                    for (int j = Count - 1; j > i; j--)
                        _items[j] = _items[j - 1];

                    _items[i] = item;
                    break;
                }
            }

            if(i == Count)
            {
                //New item is greater than all items. Insert at end.
                _items[_count] = item;
                _count++;
            }

            return i;
        }


        public void Clear() => _count = 0;
        public void RemoveLast() => _count--;
        public void RemoveLastN(int n) => _count -= n;

        public void RemoveFirstN(int n)
        {
            Array.Copy(_items, n, _items, 0, _count - n);
            _count -= n;
        }

        /// <summary>Removes by copying last element to index position. Does not retain order.</summary>
        public void RemoveBySwap(int idx)
        {
            if (idx < _count - 1)
                _items[idx] = _items[Count - 1];

            _count--;
        }

        public void Remove_RetainingOrder(int idx)
        {
            if (idx < _count - 1)
                Array.Copy(_items, idx + 1, _items, idx, _count - idx - 1);
            _count--;
        }

        /// <summary>
        /// Removes the first matching element by copying the last element to the matched position.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns> 
        public int RemoveFirstMatch(RefPredicate<T> predicate)
        {
            for (int i = 0; i < _count; i++)
            {
                if (predicate(ref _items[i]))
                {
                    _items[i] = _items[Count - 1];
                    _count--;
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes the first matching element by shifting all remaining elements one position toward array start (maintains order).
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public int RemoveFirstMatch_RetainingOrder(RefPredicate<T> predicate)
        {
            for (int i = 0; i < _count; i++)
            {
                if (predicate(ref _items[i]))
                {
                    Array.Copy(_items, i + 1, _items, i, _items.Length - i - 1);
                    _count--;
                    return i;
                }
            }

            return -1;
        }

        public int FirstMatchIdx(Func<T, bool> condition)
        {
            for (int i = 0; i < _count; i++)
            {
                if (condition(_items[i]))
                    return i;
            }

            return -1;
        }

        public bool Any(Func<T, bool> condition)
        {
            for (int i = 0; i < _count; i++)
            {
                if (condition(_items[i]))
                    return true;
            }

            return false;
        }



        /// <summary>Avg Time: O(n log(n)). Worst: O(n^2).   Operates directly on the underlying array.  Java considers it to be the fastest option for 47 to 285 items. </summary>
        /// <param name="comparer"></param>
        public void QuickSort(RefGreaterThan<T> comparer)
        {
            //Here's a good explanation of quicksort:
            //https://lamfo-unb.github.io/2019/04/21/Sorting-algorithms/
            throw new NotImplementedException();
        }

        /// <summary>Avg Time: O(n^2). Worst: O(n^2).  Operates directly on the underlying array. Java considers it to be the fastest option for less than 47 items. </summary>
        /// <param name="greaterThan"></param>
        public void InsertionSort(RefGreaterThan<T> greaterThan)
        {
            for (int i = 1; i < _count; ++i)
            {
                ref T key = ref _items[i];
                int j = i - 1;

                // Move elements of arr[0..i-1] that are greater than key
                // to one position ahead of their current position
                while (j > -1 &&  greaterThan(ref _items[j], ref key))
                {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = key;
            }
        }
    }
}
