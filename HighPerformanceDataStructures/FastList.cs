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
    /// Allows similar usage to List`, but exposes a RemoveBySwap() method that copies the last item to the Removed position instead of copying/shifting the entire array (.NET List copies/shifts making it slow).
    /// <br/><br/>If you want to work with Value Types BY REF, you can use RefList, which is nearly identical to this data structure but with by ref value semantics.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FastList<T>
    {
        /// <summary>
        /// Use Caution when mutating the Items array directly.
        /// </summary>
        public T[] Items => _items;
        T[] _items;

        public int Count { get => _count; set => _count = value; }
        int _count;

        public int Capacity => _items.Length;

        public T this[int idx] {
            get => _items [idx];
            set => _items[idx] = value;
        }

        public T Last => _items[_count - 1];

        /// <summary>Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.</summary>
        public void Add_Unsafe(T item) => _items[_count++] = item;

        /// <summary>Same as Add_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        public void Add(T item)
        {
            if (_items.Length == _count)
                IncreaseCapacity(_items.Length * 2);

            _items[_count++] = item;
        }

        /// <summary>Adds the item if its not already present in the list.</summary>
        /// <returns>True if item was added, else false</returns>
        public bool AddUnique(T itm)
        {
            for (int i = 0; i < _count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i], itm))
                    return false;
            }

            Add(itm);
            return true;
        }

        /// <summary>
        /// Performs an array copy from newItems to internal array using new items length and offset
        /// <br/><br/>SAFE: Automatically increases capacity if new items would exceed it
        /// </summary>
        /// <param name="newItems"></param>
        public void AddRange(T[] newItems)
        {
            EnsureCapacityOverhead(newItems.Length);
            Array.Copy(newItems, 0, _items, _count, newItems.Length);
            _count = _count + newItems.Length;
        }

        /// <summary>
        /// Performs an array copy from newItems to internal array using new items length and offset
        /// <br/><br/>SAFE: Automatically increases capacity if new items would exceed it
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="length">length of newItems to add</param>
        /// <param name="start">where to start in newItems array</param>
        public void AddRange(T[] newItems, int length, int start = 0)
        {
            EnsureCapacityOverhead(length - start);
            Array.Copy(newItems, start, _items, _count, length);
            _count = _count + (length - start);
        }

        /// <summary>Makes sure array size is at least capacity. If not, size is increased to exactly capacity.</summary>
        public void EnsureCapacityMinimum(int capacity)
        {
            if (_items.Length < capacity)
                IncreaseCapacity(capacity);
        }

        /// <summary>Makes sure there are at least a number of overhead slots remaining. If not, increases capacity to (Count + overhead)</summary>
        /// <param name="capacity"></param>
        public void EnsureCapacityOverhead(int overhead)
            => EnsureCapacityMinimum(_count + overhead);

        void IncreaseCapacity(int newCapacity)
        {
            var newArray = new T[newCapacity];
            Array.Copy(_items, newArray, _items.Length);
            _items = newArray; //Let GC handle the old items array
        }

        private FastList() { }

        public FastList(int capacity)
        {
            _items = new T[capacity];
        }

        /// <summary>Creates a new FastList and sets the Items to array. Sets Count explicitly. Capacity will be the array length.</summary>
        /// <param name="array"></param>
        public FastList(T[] array, int cnt)
        {
            _items = array;
            _count = cnt;
        }

        /// <summary>Assumes list is already ordered according to the given refGreaterThanTest. Finds the correct position for new item and inserts it. Avg time: O(n/2)</summary>
        /// <param name="item"></param>
        /// <returns>The index where the item was added.</returns>
        public int AddInOrderedPosition(T item, Func<T, T, bool> isGreaterThan)
        {
            if (Count == _items.Length)
                IncreaseCapacity(_items.Length * 2);

            return AddInOrderedPosition_Unsafe(item, isGreaterThan);
        }

        /// <summary>Assumes list is already ordered according to the given refGreaterThanTest. Finds the correct position for new item and inserts it. Avg time: O(n/2)</summary>
        /// <param name="item"></param>
        /// <returns>The index where the item was added.</returns>
        public int AddInOrderedPosition_Unsafe(T item, Func<T, T, bool> isGreaterThan)
        {
            int i = 0;
            for (; i < Count; i++)
            {
                if (isGreaterThan(_items[i], item))
                {
                    //Open a hole by shifting everything to the right
                    _count++;
                    for (int j = Count - 1; j > i; j--)
                        _items[j] = _items[j - 1];

                    _items[i] = item;
                    break;
                }
            }

            if (i == Count)
            {
                //New item is greater than all items. Insert at end.
                _items[_count] = item;
                _count++;
            }

            return i;
        }


        public void Clear() => _count = 0;
        /// <summary>If current capacity exceeds max capacity, the internal array will be replaced by a new one with maxCapacity.
        /// <br/><br/>Count is also decreased if it exceeds max capacity. Creates garbage.</summary>
        /// <returns>True if internal array was larger than max capacity -- a trim occurred. Else false</returns>
        public bool TrimExcess(int maxCapacity)
        {
            if (maxCapacity < _items.Length)
            {
                _items = new T[maxCapacity];
                if (_count > maxCapacity)
                    _count = maxCapacity;
                return true;
            }
            return false;
        }

        public void RemoveLast() => _count--;
        public void RemoveLastN(int n) => _count -= n;

        public void RemoveFirstN(int n)
        {
            int last = _count - 1;
            for (int i = 0; i < n; i++)
            {
                if (i < last)
                    _items[i] = _items[last--];
            }
            _count -= n;
        }

        /// <summary>Copies last item to the current index and decrements count.</summary>
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
        public int RemoveFirstMatch(Predicate<T> predicate)
        {
            for (int i = 0; i < _count; i++)
            {
                if (predicate(_items[i]))
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
        public int RemoveFirstMatch_RetainingOrder(Predicate<T> predicate)
        {
            for (int i = 0; i < _count; i++)
            {
                if (predicate(_items[i]))
                {
                    Array.Copy(_items, i + 1, _items, i, _items.Length - i - 1);
                    _count--;
                    return i;
                }
            }

            return -1;
        }

        public T FirstMatch(Func<T, bool> condition)
        {
            for (int i = 0; i < _count; i++)
            {
                if (condition(_items[i]))
                    return _items[i];
            }

            return default;
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
        public void QuickSort(Func<T,T, bool> isGreaterThan)
        {
            //Here's a good explanation of quicksort:
            //https://lamfo-unb.github.io/2019/04/21/Sorting-algorithms/
            throw new NotImplementedException();
        }

        /// <summary>Avg Time: O(n^2). Worst: O(n^2).  Operates directly on the underlying array. Java considers it to be the fastest option for less than 47 items. </summary>
        public void InsertionSort(Func<T, T, bool> isGreaterThan)
        {
            for (int i = 1; i < _count; ++i)
            {
                T key = _items[i];
                int j = i - 1;

                // Move elements of arr[0..i-1] that are greater than key
                // to one position ahead of their current position
                while (j > -1 &&  isGreaterThan(_items[j], key))
                {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = key;
            }
        }

        public FastList<T> DeepCopy()
        {
            var newList = new FastList<T>(Capacity);
            newList._count = _count;

            Array.Copy(_items, newList._items, _count);
            return newList;
        }

    }
}
