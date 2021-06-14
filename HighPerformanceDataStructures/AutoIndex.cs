using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate void EmptySlotSetter<T>(ref T item);
    public delegate bool EmptySlotTester<T>(ref T item);

    /// <summary>
    /// An auto-expanded array (similar to a list). However, this is meant for persistent index-based lookups.
    /// There will be holes in the list as items are removed and lazily replaced.
    /// Therefore the list is not meant for iteration unless you manage the holes.
    /// <br/><br/>
    /// When you invoke Add() or Add_Uninitialized(), the first available slot will be used and its index returned.
    /// <br/><br/>
    /// You must provide an emptySlotTester(ref T item) and an emptySlotEraser(ref T item). These will be used to flag and test empty slots.
    /// <br/><br/>
    /// Empty slots are skipped when enumerating the collection.
    /// <br/><br/>
    /// If you want to iterate all elements including empty slots, use the AllSlots_IncludingEmpty() method.
    /// <br/><br/>
    /// PERF NOTE: REMOVE methods invoke a Sort on the internal free slots list to keep them sorted. This makes Adds() super fast, but Removes slow.
    /// <br/><br/>CAUTION with REFERENCE TYPES: Passing by ref with reference types with pass a reference to the variable and behavior may be unexpected. This data structure is not recommended for reference types. Copy it and create a regular value semantic version.
    /// </summary>
    public class AutoIndex<T> : IEnumerable<T>
    {
        /// <summary>
        /// FOR STORING STRUCTS: You can use this to directly manipulate values.
        /// <br/>FOR STORING REFERENCE TYPES: You should NOT use the Items property at all. Instead, use the direct Indexer for safer code.
        /// <br/><br/>WARNING!!! DO NOT HOLD A REFERENCE TO ITEMS PROPERTY WHILE INVOKING OTHER METHODS!!
        /// <br/><br/>If you call other methods that replace the underlying items array (such as Add() or EnsureCapacity()), your reference to Items may become invalid!
        /// <br/>Make sure to invoke other methods BEFORE grabbing the Items property and operating on it. Discard your reference as soon as your are done mutating items.
        /// </summary>
        /// 
        public T[] Items => _items;

        protected T[] _items;
        public int Count => _count; int _count;
        public int Capacity => _items.Length;

        public ref T this[int idx] => ref _items[idx];
        public ref T Last => ref _items[_count - 1];

        List<int> _freeSlots = new List<int>(20);

        EmptySlotSetter<T> _emptySlotSetter;
        protected EmptySlotTester<T> _emptySlotTest;
        Func<T> _referenceFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <param name="emptySlotEraser">Invoked with ref to the slot from which an item was removed.  Setter should mutate the value such that the emptySlotTest will return true.</param>
        /// <param name="emptySlotTest">Invoked with ref to the slot from which an item was removed. Must return true for empty slots.</param>
        /// <param name="referenceTypeFactory">Optional: When using AutoIndex with reference types (objects), you can provide a factory method.<br/>This method will be invoked anytime new slots would return null.<br/>This means Add_Uninitialized() will NEVER produce NULL values. It will guarantee the slot is filled with an instance of the type.<br/><br/>DO NOT USE THIS WITH STRUCTS/VALUE-TYPES!!! BEHAVIOR IS UNDEFINED!</param>
        public AutoIndex(int initialCapacity, EmptySlotSetter<T> emptySlotEraser, EmptySlotTester<T> emptySlotTest, Func<T> referenceTypeFactory = null)
        {
            _items = new T[initialCapacity];
            _emptySlotSetter = emptySlotEraser;
            _emptySlotTest = emptySlotTest;
            _referenceFactory = referenceTypeFactory;

            if (emptySlotEraser == null) throw new AutoIndexException($"{nameof(emptySlotEraser)} canot be null.");
            if (emptySlotTest == null) throw new AutoIndexException($"{nameof(emptySlotTest)} canot be null.");

        }

        /// <summary>
        /// Adds the item in the first free slot (could be the end of the list).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index where the item was added (first free slot or the end of the list)</returns>
        public virtual int Add(ref T item)
        {
            int idx = Add_Uninitialized();
            _items[idx] = item;
            return idx;
        }

        /// <summary>Returns index to first free slot. After returning, position at index is considered to have a valid item in it, but you will need to set its value. </summary>
        /// <returns>Index to position where item has been added. Use index to set value of item.</returns>
        public virtual int Add_Uninitialized()
        {
            if (_freeSlots.Count > 0)
            {
                //free slots should be already sorted DESCENDING.  We grab the last item from free slots which points us to the FIRST available slot.
                int idx = _freeSlots[_freeSlots.Count - 1];
                _freeSlots.RemoveAt(_freeSlots.Count - 1);
                _count++;

                if (_referenceFactory != null)
                    _items[idx] = _referenceFactory();

                return idx;
            }
            else
            {
                if (_count == _items.Length)
                    IncreaseCapacity(_count * 2);

                if (_referenceFactory != null)
                    _items[_count] = _referenceFactory();

                return _count++;
            }
        }


        public virtual void EnsureCapacity(int capacity)
        {
            if (_items.Length < capacity)
                IncreaseCapacity(capacity);
        }

        protected virtual void IncreaseCapacity(int newCapacity)
        {
            var newArray = new T[newCapacity];
            Array.Copy(_items, newArray, _items.Length);
            _items = newArray; //Let GC handle the old items array
        }

        public virtual void Clear()
        {
            _count = 0;
            _freeSlots.Clear();
        }

        public virtual void Clear(int newCapacity)
        {
            _count = 0;
            _freeSlots.Clear();
            _freeSlots.Capacity =newCapacity;
            _items = new T[newCapacity]; ; //Let GC handle the old items array

        }

        public virtual void RemoveLast() => Remove(_count - 1);

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="match"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public virtual int Remove(RefPredicate<T> match)
        {
            for(int i = 0; i < _count; i++)
            {
                if(match(ref _items[i]))
                {
                    Remove(i);
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="idx"></param>
        public virtual void Remove(int idx)
        {
            _emptySlotSetter(ref _items[idx]);

            _count--;
            _freeSlots.Add(idx);
            _freeSlots.Sort((a, b) => b.CompareTo(a)); //Sort DESCENDING, because when we add items, we grab indexes from the end of the list which will give us the first available indexes
        }

        public virtual IEnumerable<T> AllSlots_IncludingEmpty()
        {
            for (int i = 0; i < _items.Length; i++)
                yield return _items[i];
        }


        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                if (!_emptySlotTest(ref _items[i]))
                    yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()  => this.GetEnumerator();
    }
}
