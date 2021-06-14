using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// INCOMPLETE. DO NOT USE.  I THOUGHT I NEEDED THIS AND THEN REALIZED
    /// I DON'T.  I wanted to use it with AutoIndex (aka LookupList), before realizing I already
    /// built the index to give index recycling as a built-in feature.
    /// IF YOU EVER NEED TO USE THIS, FINISH THE CODE. IT WAS NOT DONE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValuePool<T> where T : struct
    {
        List<T> _freeValues;
        Func<T> _nextValueGenerator;
        Func<T, T, int> _bestValueComparer;

        /// <summary>
        /// Returns the best available value (example, lowest number when used as number pool) or generates a new one if none are available.
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <param name="valueGenerator"></param>
        /// <param name="bestValueComparer">This comparer is used to sort the free pool values and return the first free value when one is available</param>
        public ValuePool(int initialCapacity, Func<T> valueGenerator, Func<T, T, int> bestValueComparer)
        {
            _freeValues = new List<T>(initialCapacity);
            _nextValueGenerator = valueGenerator;
            _bestValueComparer = bestValueComparer;
        }

        public T Get()
        {
            T value = _freeValues.Count > 0 
                ? _freeValues[_freeValues.Count - 1]
                : _nextValueGenerator();

            return value;
        }

        public void Return(T value)
        {
            _freeValues.Add(value);
            //_freeValues.Sort(_bestValueComparer);

        }
    }
}
