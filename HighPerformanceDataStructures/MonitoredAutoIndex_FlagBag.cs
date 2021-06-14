using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// Same behavior as AutoIndex except the items are monitored. Each time you add or remove an item,
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
    /// FlagBag Variant:
    /// <br/>
    /// Monitoring is implemented through a FastList, which maintains a list of indexes to items that match the monitored condition.
    /// When you request an enumerator to the flagged items, the FastList is iterated without branching.
    /// This can perform significantly faster than the Exhaustive variant that relies on an internal bit array.
    /// <br/><br/>
    /// Tracking monitored conditions requires searches on the FastList. This means it will be SLOWER than the Exhausive list unless
    /// *very few* of your items meet the monitored condition (resulted in a very small list of flagged items).
    /// <br/><br/>
    /// USE FLAG BAG when *very few* of your items meet the monitored condition.
    /// </summary>
    public class MonitoredAutoIndex_FlagBag<T> : AutoIndex<T>
    {
        //This variation uses an internal List instead of 

        //BitArray _monitors;

        /// <summary>Indexes to items that match the monitored condition</summary>
        FastList<int> _flaggedItemIndexes;
        RefPredicate<T> _monitoredCondition;

        public MonitoredAutoIndex_FlagBag(int initialCapacity,
            RefPredicate<T> monitoredCondition,
            EmptySlotSetter<T> emptySlotEraser,
            EmptySlotTester<T> emptySlotTest,
            Func<T> referenceTypeFactory = null)
            : base(initialCapacity, emptySlotEraser, emptySlotTest, referenceTypeFactory)
        {
            _flaggedItemIndexes = new FastList<int>(4);
        }

        public override int Add(ref T item)
        {
            int idx = base.Add(ref item);
            if (_monitoredCondition(ref item))
                _flaggedItemIndexes.Add(idx);
            return idx;
        }

        public override int Add_Uninitialized()
        {
            int idx = base.Add_Uninitialized();
            if (_monitoredCondition(ref _items[idx]))
                _flaggedItemIndexes.Add(idx);
            return idx;
        }

        protected override void IncreaseCapacity(int newCapacity)
        {
            base.IncreaseCapacity(newCapacity);
        }

        public override int Remove(RefPredicate<T> match)
        {
            int idx = base.Remove(match);
            return idx;
        }

        /// <summary>
        /// Invokes a sort on the internal FREE SLOTS list.
        /// </summary>
        /// <param name="idx"></param>
        public override void Remove(int idx)
        {
            base.Remove(idx);
            SetMonitorCondition_NotMet(idx);
        }

        /// <summary>If the item at index is not already flagged as having its monitored condition met, then it is set to met. Requires a small search.</summary>
        public void SetMonitorCondition_Met(int idx)
        {
            int flagIdx = _flaggedItemIndexes.FirstMatchIdx((i) => i == idx);
            if (flagIdx < 0)
                _flaggedItemIndexes.Add(idx);
        }

        /// <summary>If the item at index is not already flagged as having its monitored condition NOT met, then it is set to NOT met. Requires a small search.</summary>
        public void SetMonitorCondition_NotMet(int idx)
        {
            _flaggedItemIndexes.RemoveFirstMatch((i) => i == idx);
        }

        /// <summary>Sets the monitor flag based on the monitored condition.</summary>
        public void UpdateMonitor(int idx)
        {
            if (_monitoredCondition(ref _items[idx]))
                SetMonitorCondition_Met(idx);
            else
                SetMonitorCondition_NotMet(idx);
        }

        /// <summary>Could be O(n). Internally searches for flagged items. If you need this feature with better performance, consider the other MonitoredAutoIndex that relies on an internal bit array instead. It uses O(1) for this method.</summary>
        public bool IsMonitorTrue(int idx) => _flaggedItemIndexes.FirstMatchIdx((i) => i == idx) > 0;

        /// <summary>
        /// Enumerates the collection returning only items with monitored condition.
        /// <br/><br/>Skips empty slots.
        /// <br/><br/>VALUE TYPE CAUTION: Be careful when using this with value types. All other methods depend on 'ref' returns, but this method will return COPIES of the value types.
        /// </summary>
        public IEnumerable<T> GetItems_WithMonitoredCondition()
        {
            int idx = -1;
            for(int i = 0; i < _flaggedItemIndexes.Count; i++)
            {
                idx = _flaggedItemIndexes[i];
                if (!_emptySlotTest(ref _items[idx]))
                    yield return _items[idx];
            }
            
        }

        /// <summary>
        /// Enumerates the collection returning only items with monitored condition.
        /// <br/><br/>Skips empty slots.
        /// <br/><br/>Same as GetItems_ variant, except this method will automatically call UpdateMonitor() after each item has been returned and a fetch of next item is attempted.
        /// <br/>This means if you are mutating the items, the monitor for each will be updated before fetching the next item (or end-of-collection).
        /// <br/><br/>VALUE TYPE WARNING: Do NOT use this with Value Types. All other methods depend on 'ref' returns, but this method will return COPIES of the value types. That means when you mutate the items, you are only mutating the copies -- not the items in the collection.
        /// <br/><br/>GENERAL USE WARNING: Use this in a foreach loop only. Do NOT use it with anything else. And most definitely DO NOT use 'break' or 'return' inside the foreach loop. The entire process needs to iterate all items or you will get invalid state.
        /// </summary>
        public IEnumerable<T> ProcessItems_WithMonitoredCondition_NoBreak()
        {
            int idx = -1;
            for (int i = 0; i < _flaggedItemIndexes.Count; i++)
            {
                idx = _flaggedItemIndexes[i];
                yield return _items[idx];

                if(!_monitoredCondition(ref _items[idx]))
                {
                    _flaggedItemIndexes.RemoveBySwap(i);
                    i--;
                }    
            }
        }
    }
}
