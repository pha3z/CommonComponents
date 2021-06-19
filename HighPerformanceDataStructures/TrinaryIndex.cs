using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /*public delegate void EmptySlotSetByRef<T>(ref T item);
    public delegate bool EmptySlotTestByRef<T>(ref T item);*/

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
    public class TrinaryIndex<T>
    {
        //TrinaryIndex uses three bags of free slots
        //Whenever you remove an item, the index is added to the front, middle, or back bag (a bucketization technique)
        //Whenever a new item is added, the index of a free slot is fetched by first checking the front, then middle, and then back bag
        //If none of the bags has a free slot index, then Count is increased and a new item is added to the end.

        //There is an optional Sort flag which can be used to cause a sort of the bags when the LastIndex is reduced
        //significantly.  However, only benchmarks would show whether there's any benefit to this sort.
        //I suspect that in most cases, the sort will actually cause worse average performance. It really depends on how much adding and removing you're doing
        //vs how much you're simply looking up data.

        const int MIN_CAPACITY = 6;

        /// <summary>
        /// FOR STORING STRUCTS: You can use this to directly manipulate values.
        /// <br/><br/>WARNING!!! DO NOT HOLD A REFERENCE TO ITEMS PROPERTY WHILE INVOKING OTHER METHODS!!
        /// <br/><br/>If you call other methods that replace the underlying items array (such as Add() or EnsureCapacity()), your reference to Items may become invalid!
        /// <br/>Make sure to invoke other methods BEFORE grabbing the Items property and operating on it. Discard your reference as soon as your are done mutating items.
        /// </summary>
        /// 
        public T[] Items => _items;

        protected T[] _items;
        public int Count => _count;
        int _count;

        public int Capacity => _items.Length;

        /// <summary>
        /// Returns T by ref. This makes it the same behavior as C# Array Indexers.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public ref T this[int idx] => ref _items[idx];

        public int LastIndex => _lastIndex;
        int _lastIndex = 0;
        int _lastIndex_sortIndicator = 0;

        public ref T LastByRef => ref _items[_lastIndex];

        int _frontEnd = 0;
        int _middleEnd = 0;

        int _frontSlotsCount = 0;
        int _middleSlotsCount = 0;
        int _backSlotsCount = 0;

        FastList<int> _frontSlots = new FastList<int>(20);
        FastList<int> _middleSlots = new FastList<int>(20);
        FastList<int> _backSlots = new FastList<int>(20);


        EmptySlotSetByRef<T> _emptySlotSetter;
        protected EmptySlotTestByRef<T> _isEmpty;
        //Func<T> _referenceFactory;

        int _iterator;

        bool _useTrinarySort = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialCapacity">There is a minimum capacity that will be enforced even if you try to set it lower. Currently Min Capacity is 6.</param>
        /// <param name="emptySlotEraser">Invoked with ref to the slot from which an item was removed.  Setter should mutate the value such that the emptySlotTest will return true.</param>
        /// <param name="emptySlotTest">Invoked with ref to the slot from which an item was removed. Must return true for empty slots.</param>
        /// <param name="referenceTypeFactory">Optional: When using AutoIndex with reference types (objects), you can provide a factory method.<br/>This method will be invoked anytime new slots would return null.<br/>This means Add_Uninitialized() will NEVER produce NULL values. It will guarantee the slot is filled with an instance of the type.<br/><br/>DO NOT USE THIS WITH STRUCTS/VALUE-TYPES!!! BEHAVIOR IS UNDEFINED!</param>
        public TrinaryIndex(int initialCapacity, EmptySlotSetByRef<T> emptySlotEraser, EmptySlotTestByRef<T> emptySlotTest, bool useTrinarySort = false /*, Func<T> referenceTypeFactory = null*/)
        {
            if (initialCapacity < MIN_CAPACITY)
                initialCapacity = MIN_CAPACITY;

            _items = new T[initialCapacity];
            _emptySlotSetter = emptySlotEraser;
            _isEmpty = emptySlotTest;
            _useTrinarySort = useTrinarySort;
            //_referenceFactory = referenceTypeFactory;

            if (emptySlotEraser == null) throw new IndexException($"{nameof(emptySlotEraser)} canot be null.");
            if (emptySlotTest == null) throw new IndexException($"{nameof(emptySlotTest)} canot be null.");

        }

        /// <summary>Returns next free slot or -1 if no free slots available. Removes the free slot from free slots pools.</summary>
        /// <returns>next free slot or -1 if no free slots available</returns>
        int SnagFreeSlot()
        {
            //Keeps track of freeslot counts locally to avoid unnecessary memory fetches to the arrays. Performance++

            if(_frontSlotsCount > 0)
            {
                int idx = _frontSlots.Last;
                _frontSlots.RemoveLast();
                _frontSlotsCount--;
                return idx;
            }
            else if (_middleSlotsCount > 0)
            {
                int idx = _middleSlots.Last;
                _middleSlots.RemoveLast();
                _middleSlotsCount--;
                return idx;
            }
            else if (_backSlotsCount > 0)
            {
                int idx = _backSlots.Last;
                _backSlots.RemoveLast();
                _backSlotsCount--;
                return idx;
            }

            return -1;
        }

        void RecalcFreeSlotBounds()
        {
            _frontEnd = _lastIndex / 3;
            _middleEnd = _frontEnd + _frontEnd;
        }

        /// <summary>Puts the free slot into the appropriate front,middle,last bin</summary>
        void PutFreeSlot(int idx)
        {
            if (idx < _frontEnd)
            {
                _frontSlots.Add(idx);
                _frontSlotsCount = _frontSlots.Count;
            }
            else if (idx < _middleEnd)
            {
                _middleSlots.Add(idx);
                _middleSlotsCount = _middleSlots.Count;
            }
            else if (idx < _lastIndex)
            {
                _backSlots.Add(idx);
                _backSlotsCount = _backSlots.Count;
            }
        }

        /// <summary>
        /// CRITICAL: _lastIndex must be recalculated before calling this. Also do NOT sort the slot groups before calling this, because we need to use the existing bounds -- not the new ones.
        /// </summary>
        void RemoveOrphanedFreeSlots()
        {
            if (_lastIndex < _frontEnd)
            {
                _middleSlots.Clear();
                _middleSlotsCount = 0;
                _backSlots.Clear();
                _backSlotsCount = 0;

                for(int i = 0; i < _frontSlots.Count; i ++)
                {
                    if (_lastIndex < _frontSlots[i] )
                    {
                        _frontSlots.RemoveBySwap(i);
                        i--;
                    }
                }

                _frontSlotsCount = _frontSlots.Count;
                return;
            }

            if (_lastIndex < _middleEnd)
            {
                _backSlots.Clear();
                _backSlotsCount = 0;

                for (int i = 0; i < _middleSlots.Count; i++)
                {
                    if (_lastIndex < _middleSlots[i])
                    {
                        _middleSlots.RemoveBySwap(i);
                        i--;
                    }
                }

                _middleSlotsCount = _middleSlots.Count;
                return;
            }


            for (int i = 0; i < _backSlots.Count; i++)
            {
                if (_lastIndex < _backSlots[i])
                {
                    _backSlots.RemoveBySwap(i);
                    i--;
                }
            }

            _backSlotsCount = _backSlots.Count;
            return;


        }

        /// <summary>
        /// We don't want to sort everytime we reduce the last index.
        /// <br/>Instead we remember the LastIndex from the last time we sorted
        /// <br/>If the LastIndex has reduced at least 1/2 since that happened, then we resort.
        /// <br/>Also, anytime the LastIndex INCREASES, we need to reset the lastIndex sort indicator to the increased value.
        /// PERFORMANCE NOTE:
        /// This is the most expensive operation for the TinaryIndex.
        /// It also might not be necessary at all. Even if you never sorted, everything would still functional correctly
        /// thanks to the trinary slot groups.
        /// You may want to do sophisticated benchmarks where you enable and disable this with the constructor switch
        /// </summary>
        void When_LastIndexReduced_Do_SortFreeSlotsConditionally()
        {


            //If DIFFERENCE < ORIGINAL / 2, we haven't changed enough to demand a resort.
            if ((_lastIndex_sortIndicator - _lastIndex) < _lastIndex_sortIndicator / 2)
                return;

            _lastIndex_sortIndicator = _lastIndex;

            if ( _lastIndex < 3)
                return;

            int tIdx;

            for (int i = 0; i < _backSlots.Count; i++)
            {
                tIdx = _backSlots[i];
                if (tIdx > _lastIndex)
                {
                    _backSlots.RemoveBySwap(tIdx);
                    i--;
                }
            }

            for (int i = 0; i < _middleSlots.Count; i++)
            {
                tIdx = _middleSlots[i];
                if (tIdx > _middleEnd)
                {
                    if (tIdx < _lastIndex)
                        _backSlots.Add(tIdx);

                    _middleSlots.RemoveBySwap(i);
                    i--;
                }
            }


            for (int i = 0; i < _frontSlots.Count; i++)
            {
                tIdx = _frontSlots[i];
                if(tIdx > _frontEnd)
                {
                    if (tIdx < _middleEnd)
                        _middleSlots.Add(tIdx);
                    else if (tIdx < _lastIndex)
                            _backSlots.Add(tIdx);

                    _frontSlots.RemoveBySwap(i);
                    i--;
                }
            }

            _frontSlotsCount = _frontSlots.Count;
            _middleSlotsCount = _middleSlots.Count;
            _backSlotsCount = _backSlots.Count;

        }

        /// <summary>
        /// Adds the item in the first free slot (could be the end of the list).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The index where the item was added (first free slot or the end of the list)</returns>
        public virtual int Add(T item)
        {
            int idx = Add_Uninitialized_byIndex();
            _items[idx] = item;
            return idx;
        }

        public virtual ref T Add_Uninitialized_ByRef(){
            int idx = Add_Uninitialized_byIndex();
            return ref _items[idx];
        }

        /// <summary>Returns index to first free slot. After returning, position at index is considered to have a valid item in it, but you will need to set its value. </summary>
        /// <returns>Index to position where item has been added. Use index to set value of item.</returns>
        public virtual int Add_Uninitialized_byIndex()
        {
            int freeIdx = SnagFreeSlot();
            if (freeIdx > -1)
            {
                _count++;

                /*if (_referenceFactory != null)
                    _items[idx] = _referenceFactory();*/

                return freeIdx;
            }
            else
            {
                if (_count == _items.Length)
                    IncreaseCapacity(_count * 2);

                /*if (_referenceFactory != null)
                    _items[_count] = _referenceFactory();*/

                _lastIndex = _count;
                _lastIndex_sortIndicator = _lastIndex;
                _count++;

                RecalcFreeSlotBounds();
                return _lastIndex;
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
            _lastIndex = -1;
            _frontSlots.Clear();
            _middleSlots.Clear();
            _backSlots.Clear();
            _frontSlotsCount = 0;
            _middleSlotsCount = 0;
            _backSlotsCount = 0;
        }

        /// <summary>If current capacity exceeds max capacity, the internal array will be replaced by a new one with maxCapacity. Creates garbage</summary>
        /// <returns>True if internal array was larger than max capacity -- a trim occurred. Else false</returns>
        public virtual bool TrimExcess(int maxCapacity)
        {
            if (maxCapacity < MIN_CAPACITY)
                maxCapacity = MIN_CAPACITY;

            if (maxCapacity < _items.Length)
            {

                var newItems = new T[maxCapacity];
                Array.Copy(_items, 0, newItems, 0, newItems.Length);
                _items = newItems;

                if (_lastIndex > maxCapacity - 1)
                {
                    _lastIndex = maxCapacity - 1;
                    RemoveOrphanedFreeSlots();
                    RecalcFreeSlotBounds();
                    RecountElements();
                    if(_useTrinarySort)
                        When_LastIndexReduced_Do_SortFreeSlotsConditionally();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// When capacity is reduced (through TrimExcess), the truncation makes it impossible to know how many items actually remain -- because of holes.
        /// Therefore you have to recount all of the non-empty item positions.
        /// </summary>
        void RecountElements()
        {
            if (_lastIndex < 2)
            {
                _count = _lastIndex < 1 ? 0 : 1;
                return;
            }

            _count = 0;

            for (int i = 0; i < _lastIndex; i++)
            {
                if (!_isEmpty(ref _items[i]))
                    _count++;
            }

            _count++; //Add one more to account for _lastIndex because we stop prior to it.
        }

        public virtual void RemoveLast() => RemoveAt(_lastIndex);

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="match"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public virtual int Remove(RefPredicate<T> match)
        {
            int stopAt = _lastIndex + 1;
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

        /// <summary></summary>
        public virtual void RemoveAt(int idx)
        {
            _emptySlotSetter(ref _items[idx]);

            _count--;

            //If we removed the last item, we need to traverse backwards to find the next item (non-empty slot)
            //and mark it as the last index.
            if (_lastIndex == idx)
            {
                for (int i = _lastIndex - 1; i > -1; i--)
                {
                    if (!_isEmpty(ref _items[i]))
                    {
                        _lastIndex = i;
                        RemoveOrphanedFreeSlots();
                        RecalcFreeSlotBounds();
                        if (_useTrinarySort)
                            When_LastIndexReduced_Do_SortFreeSlotsConditionally(); 
                        PutFreeSlot(idx);
                        return;
                    }
                }

                //No items left. Just clear everything.
                Clear();
                return;
            }

            PutFreeSlot(idx);
        }

        /// <summary>CAUTION: Returns by value instead of by ref. You will be working on copies of the data. If you want to iterate by ref, use the ResetIterator() and Next() methods</summary>
        public virtual IEnumerable<T> All_IncludingEmpty()
        {
            int stopAt = _lastIndex + 1;
            for (int i = 0; i < stopAt; i++)
                yield return _items[i];
        }


        /// <summary>CAUTION: Returns by value instead of by ref. You will be working on copies of the data. If you want to iterate by ref, use the ResetIterator() and Next() methods</summary>
        public virtual IEnumerable<T> All()
        {
            int stopAt = _lastIndex + 1;
            for (int i = 0; i < stopAt; i++)
            {
                if (!_isEmpty(ref _items[i]))
                    yield return _items[i];
            }
        }

        /// <summary>
        /// Resets the internal items iterator. Iterating with Next() and HasNext() will skip empty slots.
        /// </summary>
        public virtual void ResetIterator() => _iterator = 0;

        public virtual bool HasNext() 
        {
            if (_iterator > _lastIndex)
                return false;

            while(_isEmpty(ref _items[_iterator]))
            {
                _iterator++;

                if (_iterator > _lastIndex)
                    return false;
            }

            return _iterator <= _lastIndex;
        }

        /// <summary>Call ResetIterator() before using Next(). CAUTION: You must check HasNext() before calling Next() to avoid faulty results. </summary>
        /// <returns></returns>
        public virtual ref T NextByRef() => ref _items[_iterator++];

        /// <summary>Call ResetIterator() before using Next(). CAUTION: You must check HasNext() before calling Next() to avoid faulty results.</summary>
        /// <returns></returns>
        public virtual T Next() => _items[_iterator++];

    }
}
