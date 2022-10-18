using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Faeric.HighPerformanceDataStructures
{
    /// <summary>
    /// Like FastList but for ValueTypes managed BY REF. Do NOT use this with Reference types -- use FastList for that purpose.
    /// <br/><br/>However, items are added strictly with AddByRef(), which eliminates unnecessary copies. This is useful for storing structs significantly larger than 4 bytes. There is still a 'ref' copy cost, which equates to a 4 byte (or 8-byte in 64-bit mode) copy anyway. So if your structs are less than 9 to 16 bytes and/or you want value-copy semantics, you may not want to use this data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RefStack<T>
    {
        readonly bool _defaultInitialize;
        T[] _items;

        public int Capacity => _items.Length;

        public ref T Peek => ref _items[_count - 1];

        private readonly T _defaultValue;

        public int Count { get => _count; set => _count = value; }
        protected int _count;

        public void Clear() => _count = 0;

        /// <summary>Decrements Count without checking if it will go negative.</summary>
        public T Pop_Unsafe()
        {
            _count--;
            return _items[_count + 1];
        }

        /// <summary>Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.</summary>
        public ref T PushByRef_Unsafe() => ref _items[_count++];

        /// <summary>Same as PushByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ref T PushByRef()
        {
            if (_count++ != _items.Length)
            {
                return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), _count);
            }

            return ref ResizeRef();
        }


        /// <summary>Makes sure there are at least a number of overhead slots remaining. If not, increases capacity to (Count + overhead)</summary>
        /// <param name="capacity"></param>
        public void EnsureCapacityOverhead(int overhead)
            => EnsureCapacity(_count + overhead);

        /// <summary>Makes sure array size is at least capacity. If not, size is increased to exactly capacity.</summary>
        public void EnsureCapacity(int ExpectedCapacity)
        {
            if (ExpectedCapacity <= _items.Length)
            {
                return;
            }

            ResizeTo(ExpectedCapacity);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        T[] ResizeTo(int newSize)
        {
            var oldArr = _items;

            _items = GC.AllocateUninitializedArray<T>(newSize);

            oldArr.AsSpan().CopyTo(_items);

            if (_defaultInitialize)
            {
                _items.AsSpan(oldArr.Length).Fill(_defaultValue);
            }

            return _items;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        ref T ResizeRef()
        {
            var oldLen = _items.Length;

            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(ResizeTo(oldLen * 2)), oldLen);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ResizeAdd(T Item)
        {
            ResizeRef() = Item;
        }

        // RefStack() { }

        public RefStack(int capacity, bool defaultInitialize = false, T defaultValue = default)
        {
            _items = GC.AllocateUninitializedArray<T>(capacity);

            _defaultInitialize = defaultInitialize;
            _defaultValue = defaultValue;

            if (_defaultInitialize)
            {
                _items.AsSpan().Fill(_defaultValue);
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public RefStack<T> DeepCopy()
        {
            var newList = new RefStack<T>(Capacity, _defaultInitialize, _defaultValue)
            {
                _count = _count
            };

            _items.AsSpan(0, _count).CopyTo(newList._items);
            
            return newList;
        }

        // void FillDefaultValues(int start)
        // {
        //     for (int i = start; i < _items.Length; i++)
        //         _items[i] = _defaultValue;
        // }
    }
}
