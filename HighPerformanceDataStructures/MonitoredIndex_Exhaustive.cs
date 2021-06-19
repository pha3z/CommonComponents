using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// Same behavior as AutoIndex except the items are monitored. Each time you add or remove an item from the list,
    /// the item is checked based on a custom predicate.
    /// The predicates return value is used to flag the item as FLAGGED or NOT_FLAGGED.
    /// If you mutate an item directly, you will need to explicitly call UpdateMonitor(itemIdx) so the internal flag can be updated.
    /// <br/><br/>
    /// You would use this when:
    /// <br/>- You have a list where the majority of items change *infrequently*.
    /// <br/>- You add and remove items *infrequently*.
    /// <br/>- You *frequently* need to process items that meet the monitored condition AND only a small-ish portion of the items meet the condition.
    /// <br/>- You are are working in critical high performance code.
    /// <br/><br/>
    /// Exhausive Variant:
    /// <br/>
    /// Monitoring is implemented through a BitArray, which requires only a single bit to flag each monitored item.
    /// When you request an enumerator to the flagged items, the BitArray is scanned.
    /// For each bit in the ON position, the monitored item is fetched and returned.
    /// <br/><br/>If you want faster processing, consider the FlagBag variant which iterates flagged items without branching. See the Usage notes on the Flag Bag variant for its weaknesses. Depending on your data, one or the other implementation is going to be faster.
    /// </summary>
    public class MonitoredIndex_Exhaustive<T> : Index<T>
    {
        BitArray _monitors;
        RefPredicate<T> _monitoredCondition;

        public MonitoredIndex_Exhaustive(int initialCapacity,
            RefPredicate<T> monitoredCondition,
            EmptySlotSetByRef<T> emptySlotEraser,
            EmptySlotTestByRef<T> emptySlotTest,
            Func<T> referenceTypeFactory = null)
            : base(initialCapacity, emptySlotEraser, emptySlotTest, referenceTypeFactory)
        {
            _monitors = new BitArray(initialCapacity, false);
        }

        public override int Add(T item)
        {
            int idx = base.Add(item);
            _monitors[idx] = _monitoredCondition(ref item);
            return idx;
        }

        public override ref T Add_Uninitialized_ByRef()
        {
            int idx = Add_Uninitialized_byIndex();
            ref T item = ref _items[idx];
            _monitors[idx] = _monitoredCondition(ref item);
            return ref item;
        }

        public override int Add_Uninitialized_byIndex()
        {
            int idx = base.Add_Uninitialized_byIndex();
            _monitors[idx] = _monitoredCondition(ref _items[idx]);
            return idx;
        }

        protected override void IncreaseCapacity(int newCapacity)
        {
            base.IncreaseCapacity(newCapacity);

            BitArray newBitArray = new BitArray(newCapacity, false);
            for(int i = 0; i == _monitors.Length; i++)
                newBitArray[i] = _monitors[i];

            _monitors = newBitArray;
        }

        /// <summary>If current capacity exceeds max capacity, the internal array will be replaced by a new one with maxCapacity. Creates garbage</summary>
        /// <returns>True if internal array was larger than max capacity -- a trim occurred. Else false</returns> 
        public override bool TrimExcess(int maxCapacity)
        {
            bool trimmed = base.TrimExcess(maxCapacity);
            if(trimmed)
                _monitors = new BitArray(maxCapacity, false);

            return trimmed;
        }

        public override int Remove(RefPredicate<T> match)
        {
            int idx = base.Remove(match);
            _monitors[idx] = false;
            return idx;
        }

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="idx"></param>
        public override void RemoveAt(int idx)
        {
            base.RemoveAt(idx);
            _monitors[idx] = false;
        }

        /// <summary>Sets the monitor flag based on the monitored condition.</summary>
        public void UpdateMonitor(int idx) => _monitors[idx] = _monitoredCondition(ref _items[idx]);

        /// <summary>Sets the monitor flag explicitly. NOTE: If something else causes the monitor to be checked (such as calling UpdateMonitor), this value will be changed.</summary>
        public void SetMonitor(int idx, bool flag) => _monitors[idx] = flag;

        public bool IsMonitorTrue(int idx) => _monitors[idx];

        /// <summary>
        /// Enumerates the collection returning only items with monitored condition.
        /// <br/><br/>Skips empty slots.
        /// <br/><br/>CAUTION: Returns by value instead of by ref. You will be working on copies of the data.
        /// </summary>
        public IEnumerable<T> GetItems_WithMonitoredCondition()
        {
            for(int i = 0; i < Count; i++)
            {
                if (_monitors[i] == true && !_isEmpty(ref _items[i]))
                    yield return _items[i];
            }
        }

        /// <summary>
        /// Enumerates the collection returning only items with monitored condition.
        /// <br/><br/>Skips empty slots.
        /// <br/><br/>Same as GetItems_ variant, except this method will automatically call UpdateMonitor() after each item has been returned and a fetch of next item is attempted.
        /// <br/>This means if you are mutating the items, the monitor for each will be updated before fetching the next item (or end-of-collection).
        /// <br/><br/>CAUTION: Returns by value instead of by ref. You will be working on copies of the data.
        /// <br/><br/>GENERAL USE WARNING: Use this in a foreach loop only. Do NOT use it with anything else. And most definitely DO NOT use 'break' or 'return' inside the foreach loop. The entire process needs to iterate all items or you will get invalid state.
        /// </summary>
        public IEnumerable<T> ProcessItems_WithMonitoredCondition_NoBreak()
        {
            for (int i = 0; i < Count; i++)
            {
                if (_monitors[i] == true && !_isEmpty(ref _items[i]))
                    yield return _items[i];

                UpdateMonitor(i);
            }
        }

    }
}
