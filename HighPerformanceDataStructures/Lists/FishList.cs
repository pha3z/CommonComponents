using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// Fixed Index with Stack of Holes - FISH<br/>
    /// FishList is an excellent choice when you want to implement your own linked-list or tree-style structure as a fixed structure in an array to achieve high memory performance.<br/>
    /// In a fish list, items never move, which enables consumer to make O(1) lookups to items using item index (position in the list).<br/>
    /// Item Lookup, Add, and Remove all are O(1) operations.<br/><br/>
    /// Traversal/Iteration is the responsibility of the consumer (implementor of T).  Items should contain fields to link between neighbors.<br/>
    /// Implementation details:<br/>
    /// Internally uses only one single array -- no additional data structures.<br/>
    /// When the array is initialized, there are no holes and the item count is 0.<br/>
    /// When an item is added, if the are no holes, the item is added after the last (rightmost) item and count is incremented</br><br/>
    /// When item is removed, a hole may be formed.<br/>
    /// Holes link to each other to form a Stack where the newest hole is the top of the stack and the oldest hole is at the bottom.</br>
    /// This makes holes FILO. When a node is added, if there are existing holes, the newest hole becomes the location of the new item -- effectively the hole is popped off the top of the stack.<br/><br/>
    /// PERFORMANCE NOTES:<br/>
    /// As the ratio of holes to items increases, performance may decline because there is no compaction.<br/>
    /// Your experience may very depending on the size of T. Holes shoud cause more noticable impact when T is small. As the size of T approaches the size of a cacheline, the cost of having holes converges toward 0.<br/>
    /// </summary>
    /// <typeparam name="T">T must be at least 4 bytes total.</typeparam>
    public class FishList<T> where T : struct
    {
        /// <summary>
        /// You can mutate item values directly, but use the FishList public methods for adding and removing items.
        /// </summary>
        public T[] Items => _items;
        T[] _items;

        public int Count;

        public int Capacity => _items.Length;

        public T this[int idx]
        {
            get => _items[idx];
            set => _items[idx] = value;
        }

        /// <summary>
        /// If you imagine the slice of array containing items with array position 0 being the leftmost element, then this is the position of the rightmost item.
        /// This is NOT necessarily the newest (most recently added) item.</summary>
        int _rightBoundItem = -1;
        int _newestHole = -1;

        private FishList() { }

        public FishList(int capacity)
        {
            _items = new T[capacity];
        }

        /// <summary>Creates a new FishList and sets the Items to array. Sets Count explicitly. Capacity will be the array length.</summary>
        /// <param name="array"></param>
        public FishList(T[] array, int cnt)
        {
            _items = array;
            Count = cnt;
        }

        /// <summary>Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.</summary>
        public void Add_Unchecked(T item)
        {
            _items[AquireNextInsertPosition()] = item;
        }

        /// <summary>Finds an available hole/array-element position and returns a reference to it. You can then write to it. Does not check available space in array--use with caution. If array is full, an exception will occur.</summary>
        public ref T AddByRef_Unchecked()
        {
            return ref _items[AquireNextInsertPosition()];
        }

        /// <summary>Checks capacity before adding the item. Capacity will be doubled if more space is needed.</summary>
        public void Add(T item)
        {
            if (_items.Length == Count)
                IncreaseCapacity(_items.Length * 2);

            _items[AquireNextInsertPosition()] = item;
        }

        /// <summary>Finds an available hole/array-element position and returns a reference to it. You can then write to it.</summary>
        public ref T AddByRef()
        {
            if (_items.Length == Count)
                IncreaseCapacity(_items.Length * 2);

            return ref _items[AquireNextInsertPosition()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe int AquireNextInsertPosition()
        {
            Count++;
            if (_newestHole == -1)
                return _rightBoundItem++;
            else
            {
                int insertPosition = _newestHole;
                ref T newestHole = ref _items[_newestHole];
                _newestHole = Unsafe.As<T, int>(ref newestHole);
                return insertPosition;
            }
        }

        /// <summary>Adds the item if its not already present in the list. O(n) -- EqualityComparer&ltT&gt;.Default is used to compare add item to each existing item in the array. If an equal item is found, the new item is not added.</summary>
        /// <returns>True if item was added, else false</returns>
        public bool AddUnique(T itm)
        {
            for (int i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i], itm))
                    return false;
            }

            Add(itm);
            return true;
        }

        /// <summary>
        /// Appends newItems to existing items in the internal array using Array.Copy.<br/>
        /// NOTE: This circumvents the internal holes stack. Any holes in the existing<br/>
        /// array will remain untouched, because the new items are all appended after the<br/>
        /// existing items.<br/>
        /// Using this method repeatedly may cause the array to expand indefinitely since holes do not get filled.
        /// <br/><br/>SAFE: Automatically increases capacity if new items would exceed it
        /// </summary>
        /// <param name="newItems"></param>
        /*public void AppendItems(T[] newItems)
        {
            //Is EnsureCapacityOverhead the correct method to call here?
            EnsureCapacityOverhead(newItems.Length);
            Array.Copy(newItems, 0, _items, _count, newItems.Length);
            _count = _count + newItems.Length;
        }*/

        /// <summary>Makes sure array size is at least capacity. If not, size is increased to exactly capacity.</summary>
        public void EnsureCapacityMinimum(int capacity)
        {
            if (_items.Length < capacity)
                IncreaseCapacity(capacity);
        }

        /// <summary>Makes sure there are at least a number of overhead slots remaining. If not, increases capacity to (Count + overhead)</summary>
        /// <param name="capacity"></param>
        public void EnsureCapacityOverhead(int overhead)
            => EnsureCapacityMinimum(Count + overhead);

        void IncreaseCapacity(int newCapacity)
        {
            var newArray = new T[newCapacity];
            Array.Copy(_items, newArray, _items.Length);
            _items = newArray; //Let GC handle the old items array
        }

        /// <summary>Clears all items from the array. This is hyperfast. It sets Count to 0 and resets the newestHole index.</summary>
        public void Clear()
        {
            Count = 0;
            _newestHole = -1;
            _rightBoundItem = -1;
        }

        /// <summary>Removes the item at given index.</summary>
        public void Remove(int idx)
        {
            Count--;

            if(idx == _rightBoundItem)
            {
                _rightBoundItem--;    
            }
            else
            {
                ref T item = ref _items[idx];
                ref int hole = ref  Unsafe.As<T, int>(ref item);
                hole = _newestHole;
                _newestHole = idx;
            }
        }

        /// <summary>
        /// Removes the first matching element by copying the last element to the matched position.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns> 
        public bool RemoveFirstMatch(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate(_items[i]))
                {
                    _items[i] = _items[Count - 1];
                    Remove(i);
                    return true;
                }
            }

            return false;
        }

        public T FirstMatch(Func<T, bool> condition)
        {
            for (int i = 0; i < Count; i++)
            {
                if (condition(_items[i]))
                    return _items[i];
            }

            return default;
        }

        public int FirstMatchIdx(Func<T, bool> condition)
        {
            for (int i = 0; i < Count; i++)
            {
                if (condition(_items[i]))
                    return i;
            }

            return -1;
        }

        public bool Any(Func<T, bool> condition)
        {
            for (int i = 0; i < Count; i++)
            {
                if (condition(_items[i]))
                    return true;
            }

            return false;
        }

        public FishList<T> DeepCopy()
        {
            throw new NotImplementedException();
            //Flesh out this code

            var newList = new FishList<T>(Capacity);
            newList.Count = Count;

            Array.Copy(_items, newList._items, Count);
            return newList;
        }
    }
}
