using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate void EmptySlotSetByRef<T>(ref T item);
    public delegate bool EmptySlotTestByRef<T>(ref T item);

    /// <summary>
    /// An auto-expanded array meant for persistent index-based lookups.
    /// Holes remain when items are removed, so indexes stay persistent.
    /// <br/><br/>
    /// To enumerate the list, use the All() or All_IncludingEmpty(). NOTE: Enumeration returns values by value instead of by ref. Be aware of this!!
    /// By design, IEnumerable is NOT implemented. This makes it so the dot (.) operator does not show you all the LINQ junk you don't want to see.
    /// <br/><br/>
    /// When you invoke Add() or Add_Uninitialized(), the first available slot will be used and its index returned.
    /// <br/><br/>
    /// You must provide an emptySlotTester(ref T item) and an emptySlotEraser(ref T item). These will be used to flag and test empty slots.
    /// <br/><br/>
    /// PERF NOTE: REMOVE methods invoke a Sort on the internal free slots list to keep them sorted. This makes frequent Add/Remove rather slow. If you expect to add and remove frequently, consider a HashSet or Dictionary instead. The Index data structure is intended for situations where you are doing a lot more lookups than add/removes.
    /// <br/><br/>WARNING ABOUT REFERENCE TYPES: The behavior is not tested for reference types. The EmptySlot delegates pass by ref... not sure what will happen if you use these with reference types.
    /// </summary>
    public class Index<T>
    {
        //PERFORMANCE IDEA
        //We sort Remove so that Adds will always try to go for the first slot.
        //You might get better overall performance by grabbing randomly from the bag of freeslots
        //We should have this as a configuration option perhaps??
        //Like you configure a delegate for the Remove() method
        //Configure to work on first-slot priority or slot-grabbag
        //Yeah
        //Or its modal even
        //in modal configuration
        //the Remove delegate would check to see if the number of free slots compared to count of items
        //If they are equal, then the new freeslot is added.
        //  and then the freeslots array is sorted so that first slot becomes prioritized
        //If there are more free slots than items, then the we assume the free slots are already sorted (from the last removal)
        //and we add the new free slot in sorted order so everything remains sorted
        //
        //We may want a scalar instead of just "half of the count"
        //Not perfect idea but it could be a good compromise for performance
        //
        

        /// <summary>
        /// FOR STORING STRUCTS: You can use this to directly manipulate values.
        /// <br/><br/>WARNING!!! DO NOT HOLD A REFERENCE TO ITEMS PROPERTY WHILE INVOKING OTHER METHODS!!
        /// <br/><br/>If you call other methods that replace the underlying items array (such as Add() or EnsureCapacity()), your reference to Items may become invalid!
        /// <br/>Make sure to invoke other methods BEFORE grabbing the Items property and operating on it. Discard your reference as soon as your are done mutating items.
        /// </summary>
        /// 
        public T[] Items => _items;

        protected T[] _items;
        public int Count = 0;
        public int Capacity => _items.Length;

        /// <summary>
        /// Returns T by ref. This makes it the same behavior as C# Array Indexers.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public ref T this[int idx] => ref _items[idx];

        public int LastIndex = 0;
        public ref T LastByRef => ref _items[LastIndex];

        List<int> _freeSlots = new List<int>(20);

        EmptySlotSetByRef<T> _emptySlotSetter;
        protected EmptySlotTestByRef<T> _isEmpty;
        Func<T> _referenceFactory;

        int _iterator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <param name="emptySlotEraser">Invoked with ref to the slot from which an item was removed.  Setter should mutate the value such that the emptySlotTest will return true.</param>
        /// <param name="emptySlotTest">Invoked with ref to the slot from which an item was removed. Must return true for empty slots.</param>
        /// <param name="referenceTypeFactory">Optional: When using AutoIndex with reference types (objects), you can provide a factory method.<br/>This method will be invoked anytime new slots would return null.<br/>This means Add_Uninitialized() will NEVER produce NULL values. It will guarantee the slot is filled with an instance of the type.<br/><br/>DO NOT USE THIS WITH STRUCTS/VALUE-TYPES!!! BEHAVIOR IS UNDEFINED!</param>
        public Index(int initialCapacity, EmptySlotSetByRef<T> emptySlotEraser, EmptySlotTestByRef<T> emptySlotTest, Func<T> referenceTypeFactory = null)
        {
            _items = new T[initialCapacity];
            _emptySlotSetter = emptySlotEraser;
            _isEmpty = emptySlotTest;
            _referenceFactory = referenceTypeFactory;

            if (emptySlotEraser == null) throw new IndexException($"{nameof(emptySlotEraser)} canot be null.");
            if (emptySlotTest == null) throw new IndexException($"{nameof(emptySlotTest)} canot be null.");

        }

        /// <summary>
        /// Adds the item in the first free slot (could be the end of the list).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index where the item was added (first free slot or the end of the list)</returns>
        public virtual int Add(T item)
        {
            int idx = Add_Uninitialized();
            _items[idx] = item;
            return idx;
        }

        public virtual ref T AddByRef(){
            int idx = Add_Uninitialized();
            return ref _items[idx];
        }

        /// <summary>Returns index to first free slot. After returning, position at index is considered to have a valid item in it, but you will need to set its value. </summary>
        /// <returns>Index to position where item has been added. Use index to set value of item.</returns>
        public virtual int Add_Uninitialized()
        {
            if (_freeSlots.Count > 0)
            {
                //free slots should be already sorted DESCENDING.  We grab the last item from free slots which points us to the FIRST available slot.
                int idx = _freeSlots[_freeSlots.Count - 1];
                _freeSlots.RemoveAt(idx);
                Count++;

                if (_referenceFactory != null)
                    _items[idx] = _referenceFactory();

                return idx;
            }
            else
            {
                if (Count == _items.Length)
                    IncreaseCapacity(Count * 2);

                if (_referenceFactory != null)
                    _items[Count] = _referenceFactory();

                LastIndex = Count;
                return Count++;
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
            Count = 0;
            LastIndex = -1;
            _freeSlots.Clear();
        }

        /// <summary>If current capacity exceeds max capacity, the internal array will be replaced by a new one with maxCapacity. Creates garbage</summary>
        /// <returns>True if internal array was larger than max capacity -- a trim occurred. Else false</returns>
        public virtual bool TrimExcess(int maxCapacity)
        {
            if (maxCapacity < _items.Length)
            {
                _items = new T[maxCapacity];
                _freeSlots = new List<int>(maxCapacity);
                if (Count > maxCapacity)
                {
                    Count = maxCapacity;
                    LastIndex = Count - 1;
                }
                return true;
            }

            return false;
        }

        public virtual void RemoveLast() => RemoveAt(LastIndex);

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="match"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public virtual int Remove(RefPredicate<T> match)
        {
            int stopAt = LastIndex + 1;
            for (int i = 0; i < stopAt; i++)
            {
                if (match(ref _items[i]))
                {
                    RemoveAt(i);
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="idx"></param>
        public virtual void RemoveAt(int idx)
        {
            _emptySlotSetter(ref _items[idx]);

            Count--;
            _freeSlots.Add(idx);
            _freeSlots.Sort((a, b) => b.CompareTo(a)); //Sort DESCENDING, because when we add items, we grab indexes from the end of the list which will give us the first available indexes

            //If we removed the last item, we need to traverse backwards to find the next item (non-empty slot)
            //and mark it as the last index.
            if (LastIndex == idx)
            {
                for(int i = LastIndex - 1; i > -1; i--)
                {
                    if (!_isEmpty(ref _items[i]))
                    {
                        LastIndex = i;
                        return;
                    }
                }

                LastIndex = -1;
            }
        }

        /// <summary>CAUTION: Returns by value instead of by ref. You will be working on copies of the data. If you want to iterate by ref, use the ResetIterator() and Next() methods</summary>
        public virtual IEnumerable<T> All_IncludingEmpty()
        {
            int stopAt = LastIndex + 1;
            for (int i = 0; i < stopAt; i++)
                yield return _items[i];
        }


        /// <summary>CAUTION: Returns by value instead of by ref. You will be working on copies of the data. If you want to iterate by ref, use the ResetIterator() and Next() methods</summary>
        public virtual IEnumerable<T> All()
        {
            int stopAt = LastIndex + 1;
            for (int i = 0; i < stopAt; i++)
            {
                if (!_isEmpty(ref _items[i]))
                    yield return _items[i];
            }
        }

        public virtual void ResetIterator() => _iterator = 0;

        public virtual bool NextIsEoL() 
        {
            if (_iterator > LastIndex)
                return true;

            while(_isEmpty(ref _items[_iterator]))
            {
                _iterator++;

                if (_iterator > LastIndex)
                    return true;
            }

            return _iterator > LastIndex;
        }

        /// <summary>Call ResetIterator() before using Next(). Also, Next() will NOT check for End-of-List. You must check the value of NextIsEoL() before invoking Next()</summary>
        /// <returns></returns>
        public virtual ref T NextByRef() => ref _items[_iterator++];

        /// <summary>Call ResetIterator() before using Next(). Also, Next() will NOT check for End-of-List. You must check the value of NextIsEoL() before invoking Next()</summary>
        /// <returns></returns>
        public virtual T Next() => _items[_iterator++];

    }
}
