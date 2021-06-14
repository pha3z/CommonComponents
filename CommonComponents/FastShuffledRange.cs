using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// Works by building an internal array of integers when constructed.
    /// An internal iterator starts at the beginning of the array
    /// and calls to NextValue() return the value and advance to next.
    /// Because the initial calculations are done only during construction,
    /// the rest of the access methods will be limited only by memory read performance.
    /// When you traverse through the shuffled array from start to end (instead of dynamically jumping around)
    /// the performance will be extremely high since entire blocks will be hotcached as you move forward.
    /// Does not rely on LINQ or other systems, so as to ensure predictability.
    /// The only Garbage generatored for GC is a temporary array during the constructor call.
    /// It becomes available to GC after constructor returns. Bear in mind, its not GC'd until the GC actually runs.
    /// </summary>
    public class FastShuffledRange
    {
        int[] _deck;
        int _position;

        public int Length => _deck.Length;
        public bool HasValuesRemaining => _position < _deck.Length;

        public FastShuffledRange(int start, int count, Random rnd)
        {
            _deck = new int[count];
            var _possibleNumbers = new int[count];
            int remainingCnt = count;

            for (int i = 0; i < count; i++)
                _possibleNumbers[i] = i;

            int iShuffled = 0;
            while (remainingCnt > 0)
            {
                int r = rnd.Next(remainingCnt);
                _deck[iShuffled] = _possibleNumbers[r];

                //Swap the end element to the removed position and decrement remainder.
                _possibleNumbers[r] = _possibleNumbers[remainingCnt - 1];
                remainingCnt--;

                iShuffled++;
            }

        }

        public FastShuffledRange(int start, int count) 
            : this(start, count, new Random()) { }

        /// <summary>
        /// Set to 0 to reset to the beginning of the internal array
        /// </summary>
        /// <param name="index"></param>
        public void SetPosition(int index) => _position = index;

        /// <summary>
        /// Rewinds N steps, clamping at 0 (start of array).
        /// </summary>
        /// <param name="steps"></param>
        public void Rewind(int steps)
        {
            _position -= steps;
            if (_position < 0)
                _position = 0;
        }

        public int NextValue()
        {
            int val = _deck[_position];
            _position++;
            return val;
        }


    }
}
