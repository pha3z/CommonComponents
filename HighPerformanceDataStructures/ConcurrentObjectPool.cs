using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// THREAD SAFE. Relies on a .NET ConcurrentBag
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentObjectPool<T>
    {
        private readonly ConcurrentBag<T> _objects;
        private readonly Func<T> _objectGenerator;

        public ConcurrentObjectPool(Func<T> objectGenerator)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objects = new ConcurrentBag<T>();
        }

        public T Checkout() => _objects.TryTake(out T item) ? item : _objectGenerator();

        public void Checkin(T item) => _objects.Add(item);
    }
}
