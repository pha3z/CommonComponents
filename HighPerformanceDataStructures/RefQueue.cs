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
    /// A queue using a circular array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RefQueue<T>   where T : struct 
    {
        T[] _items;

        int _first = 0;
        public int Last { get; private set; } = 0;

        public int Count => _count; int _count  = 0;
        public int Capacity => _items.Length;

        public ref T this[int idx] => ref _items[idx];

        /// <summary>Same as AddByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        public void Enqueue(T item)
        {
            if (_items.Length == _count)
                IncreaseCapacity(_items.Length * 2);

            _count++;
            Last++;

            if (Last > _items.Length)
                Last = 0;

            _items[Last] = item;
        }

        /// <summary>Same as AddByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        public ref T EnqueueByRef()
        {
            if (_items.Length == _count)
                IncreaseCapacity(_items.Length * 2);

            _count++;
            Last++;

            if (Last > _items.Length)
                Last = 0;

            return ref _items[Last];
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

            if (_first < Last)
            {
                Array.Copy(_items, newArray, _items.Length);
            }
            else
            {   //There's a loop. Normalize it.

                int newIdx = 0;

                for(int i = _first; i < _count; i++)
                    newArray[newIdx++] = _items[i];

                int stopAt = Last + 1;
                for (int i = 0; i < stopAt; i++)
                    newArray[newIdx++] = _items[i];

                _first = 0;
                Last = _count;
            }
            
            _items = newArray; //Let GC handle the old items array
        }



        private RefQueue() { }

        public RefQueue(int capacity)
        {
            _items = new T[capacity];
        }


        public void Clear()
        {
            _count = 0; _first = 0; Last = -1;
        }

        /// <summary>Does not clamp n to count of items. USE THIS WITH GREAT CAUTION.  HONESTLY JUST DONT USE IT.</summary>
        public void DequeueN_Unsafe(int n)
        {
            _first += n;

            if (_first > _count - 1)
                _first = (_first - _count);

            _count -= n;
        }

        /// <summary>Clamps n to Count of items</summary>
        public void DequeueN(int n)
        {
            if (n > _count)
                n = _count;

            DequeueN_Unsafe(n);
        }

        /// <summary>If items are laid out in a loop (First > Last), rearranges the items so that First is at index 0 and Last is the last item.
        /// <br/><br/>IMPORTANT: For simplicity, this algorithm assumes the capacity was doubled just prior to calling it. The newly created space is used</summary>
        /// Fail We don't even need this
        /// Since we would call this after increasing the capacity and copying everything into the new array
        /// we might as well just copy everything in order
        /*void Normalize_DEPRECATED()
        {
            int Divide_RoundedUp(int int1, int int2)
            {
                int result = (int1 / int2);
                if (int1 % int2 != 0) { result++; }
                return result;
            }

            T tItem;

            if(First > Last)
            {
                //Exclusive bounds. Ie gapStartEx is the item before the first gap item.
                int gapStartEx = Last;
                int gapEndEx = First;

                //If the numbers at beginning of array are greater than half the space...
                //We will have to move the smaller set (the one after the gap) to the front (before the gap)
                //by swapping them.
                if (gapStartEx > Divide_RoundedUp(_count, 2))
                {
                    for (int i = gapEndEx; i < _count; i++)
                    {
                        tItem = _items[i - gapEndEx]; //Temporarily store item from beginning set
                        _items[i - gapEndEx] = _items[i]; //Move the ending item to beginning set
                        _items[i] = tItem; //Move the temporary item to ending set
                    }

                    //We might have some stragglers on the front now because the front was greater than the back/end set.
                    int iNextFrontPosition = (_count - gapStartEx);

                    //Now swap the stragglers with the new ending set
                    

                }
                for(int i = 0; i < Last; i ++)
                    _items[i] = _items[First + i];

                //some middle section of the set now begins at idx Last.

                //for (int i = Last + 1; i < )
            }
        }*/
    }
}
