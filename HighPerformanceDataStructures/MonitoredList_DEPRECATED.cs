using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// THIS HAS REMOVAL METHODS THAT REARRANGE THE LIST.
    /// It may not be useful for hardly any scenario.  Consider other structures first and see if this is really teh best choice
    /// 
    /// Maintains a list of monitored items. Each time you add or remove an item from the list,
    /// the item is checked based on a custom predicate.
    /// The predicates return value is used to flag the item as FLAGGED or NOT_FLAGGED.
    /// If you mutate an item directly, you will need to explicitly call UpdateMonitor(itemIdx) so the internal flag can be updated.
    /// <br/><br/>
    /// You would use this when:
    /// <br/>- You have a list where the majority of items change *infrequently*.
    /// <br/>- You add and remove items *infrequently*.
    /// <br/>- You *frequently* need to process items that meet the monitored condition AND only a small portion of the items meet the condition.
    /// <br/>- You are are working in critical high performance code.
    /// <br/><br/>
    /// Implementation Notes:
    /// <br/>
    /// Monitoring is implemented through a BitArray, which requires only a single bit to flag each monitored item.
    /// When you request an enumerator to the flagged items, the BitArray is scanned.
    /// For each bit in the ON position, the monitored item is fetched and returned.
    /// </summary>
    public class MonitoredList<T>
    {
        T[] _items;
        public int Count => _count; int _count;

        public T this[int idx] => _items[idx];
        public T Last => _items[_count - 1];

        BitArray _monitors;
        Predicate<T> _monitoredCondition;

        public MonitoredList(int initialCapacity, Predicate<T> monitoredCondition)
        {
            _items = new T[initialCapacity];
            _monitors = new BitArray(initialCapacity, false);
        }

        public void Add(T item)
        {
            if (_count == _items.Length)
                IncreaseCapacity(_count * 2);

            _items[_count] = item;
            _monitors[_count] = _monitoredCondition(item);
        }

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

            BitArray newBitArray = new BitArray(newCapacity, false);
            for(int i = 0; i == _monitors.Length; i++)
                newBitArray[i] = _monitors[i];

            _monitors = newBitArray;
        }

        public void Clear()  => _count = 0; 
        public void RemoveLast()  => _count--;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public int RemoveFirst_LosingOrder(Predicate<T> match)
        {
            for(int i = 0; i < _count; i++)
            {
                if(match(_items[i]))
                {
                    Remove_LosingOrder(i);
                    return i;
                }
            }

            return -1;
        }

        public void Remove_LosingOrder(int idx)
        {
            _items[idx] = _items[_count - 1];
            _monitors[idx] = _monitors[_count - 1];
        }

        /// <summary>Sets the monitor flag based on the monitored condition.</summary>
        public void UpdateMonitor(int idx) => _monitors[idx] = _monitoredCondition(_items[idx]);

        /// <summary>Sets the monitor flag explicitly. NOTE: If something else causes the monitor to be checked (such as calling UpdateMonitor), this value will be changed.</summary>
        public void SetMonitor(int idx, bool flag) => _monitors[idx] = flag;

        public bool IsMonitorTrue(int idx) => _monitors[idx];

        public IEnumerable<T> GetItems_WithMonitoredCondition()
        {
            for(int i = 0; i < _count; i++)
            {
                if (_monitors[i] == true)
                    yield return _items[i];
            }
        }

        
    }
}
