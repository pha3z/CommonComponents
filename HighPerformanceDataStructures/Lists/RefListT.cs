using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate bool RefPredicate<T>(ref T item);
    public delegate bool RefGreaterThan<T>(ref T item1, ref T item2);

    /// <summary>
    /// Like FastList but for ValueTypes managed BY REF. Do NOT use this with Reference types -- use FastList for that purpose.
    /// <br/><br/>However, items are added strictly with AddByRef(), which eliminates unnecessary copies. This is useful for storing structs significantly larger than 4 bytes. There is still a 'ref' copy cost, which equates to a 4 byte (or 8-byte in 64-bit mode) copy anyway. So if your structs are less than 9 to 16 bytes and/or you want value-copy semantics, you may not want to use this data structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RefList<T>
    {
        readonly bool _defaultInitialize;

        /// <summary>
        /// Use Caution when mutating the Items array directly.
        /// </summary>
        public T[] Items => _items;
        T[] _items;

        public int Capacity => _items.Length;

        public ref T this[int idx] => ref _items[idx];

        public ref T Last => ref _items[_count - 1];

        private readonly T _defaultValue;

        public int Count { get => _count; set => _count = value; }
        protected int _count;

        public void Clear() => _count = 0;

        /// <summary>Adds an item without checking capacity first. Will throw Array Out-of-Bounds exception if there is no space left at the end of the internal array.</summary>
        public ref T AddByRef_Unsafe() => ref _items[_count++];

        /// <summary>Same as AddByRef_Unsafe() except that the capacity will be checked before an item is added. Capacity will be doubled if more space is needed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ref T AddByRef()
        {
            if (_count++ != _items.Length)
            {
                return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), _count);
            }

            return ref ResizeRef();
        }

        /// <summary>
        /// Performs an array copy from newItems to internal array using new items length and offset
        /// <br/><br/>SAFE: Automatically increases capacity if new items would exceed it
        /// </summary>
        /// <param name="newItems"></param>
        public void AddRange(T[] newItems)
        {
            EnsureCapacityOverhead(newItems.Length);
            Array.Copy(newItems, 0, _items, _count, newItems.Length);
            _count = _count + newItems.Length;
        }

        /// <summary>
        /// Performs an array copy from newItems to internal array using new items length and offset
        /// <br/><br/>SAFE: Automatically increases capacity if new items would exceed it
        /// </summary>
        /// <param name="newItems"></param>
        /// <param name="length">length of newItems to add</param>
        /// <param name="start">where to start in newItems array</param>
        public void AddRange(T[] newItems, int length, int start = 0)
        {
            EnsureCapacityOverhead(length - start);
            Array.Copy(newItems, start, _items, _count, length);
            _count = _count + (length - start);
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
        private T[] ResizeTo(int newSize)
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
        private ref T ResizeRef()
        {
            var oldLen = _items.Length;

            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(ResizeTo(oldLen * 2)), oldLen);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ResizeAdd(T Item)
        {
            ResizeRef() = Item;
        }

        // private RefList() { }

        public RefList(int capacity, bool defaultInitialize = false, T defaultValue = default)
        {
            _items = GC.AllocateUninitializedArray<T>(capacity);

            _defaultInitialize = defaultInitialize;
            _defaultValue = defaultValue;
            
            if (_defaultInitialize)
            {
                _items.AsSpan().Fill(_defaultValue);
            }
        }

        /// <summary>
        /// This overload allows a default value that will be used to fill new initialized values of the underlying array.
        /// <br/>NOTE: Removed items will retain their value. If you want removed items to be changed to their default value, you will need to do it explicitly.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="defaultValue">The default value will be copied into all array slots when underlying array is initialized. It is also copied into new slots when the underlying array is copied (due to a capacity increase).</param>
        // public RefList(int capacity, T defaultValue)
        // {
        //     _defaultValue = defaultValue;
        //     DefaultInitialize = true;
        //     _items = new T[capacity];
        //     FillDefaultValues(0);
        // }

        public interface IGreaterThan
        {
            //Left is greater than right
            public bool LeftIsGreaterThanRight(ref T Left, ref T Right);
        }
        
        private readonly struct GreaterThanDelWrapper: IGreaterThan
        {
            private readonly RefGreaterThan<T> Del;

            public GreaterThanDelWrapper(RefGreaterThan<T> del)
            {
                Del = del;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public bool LeftIsGreaterThanRight(ref T left, ref T right)
            {
                return Del(ref left, ref right);
            }
        }

        /// <summary>Assumes list is already ordered according to the given refGreaterThanTest. Finds the correct position for new item and inserts it. Avg time: O(n/2)</summary>
        /// <param name="item"></param>
        /// <param name="refGreaterThanTest"></param>
        /// <returns>The index where the item was added.</returns>
        public ref T AddInOrderedPosition<GreaterThanT>(T item, GreaterThanT refGreaterThanTest)
            where GreaterThanT : IGreaterThan
        {
            var oldCount = Count++;
            
            EnsureCapacity(Count);
            
            ref var current = ref MemoryMarshal.GetArrayDataReference(_items);

            ref var lastOffsetByOne = ref Unsafe.Add(ref current, oldCount);

            ref var last = ref Unsafe.Subtract(ref lastOffsetByOne, 1);

            //Is new item greater than all items?
            if (refGreaterThanTest.LeftIsGreaterThanRight(ref item, ref last))
            {
                //The JIT should invert the branch, making this uncommon
                //This allows the common branch to avoid a jmp, and help with
                //branch prediction
                goto InsertLast;
            }
            
            for (; !Unsafe.AreSame(ref current, ref last); current = ref Unsafe.Add(ref current, 1))
            {
                if (refGreaterThanTest.LeftIsGreaterThanRight(ref item, ref current))
                {
                    continue;
                }

                break;
            }
            
            //[0, 1, 2]
            var moveCount = (int) Unsafe.ByteOffset(ref current, ref lastOffsetByOne) / Unsafe.SizeOf<T>();

            var origin = MemoryMarshal.CreateSpan(ref current, moveCount);

            var destination = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref current, 1), moveCount);
                
            origin.CopyTo(destination);
            
            current = item;

            return ref current;
            
            InsertLast:
            lastOffsetByOne = item;

            return ref lastOffsetByOne;
        }


        /// <summary>If current capacity exceeds max capacity, the internal array will be replaced by a new one with maxCapacity.
        /// <br/><br/>Count is also decreased if it exceeds max capacity. Creates garbage.</summary>
        /// <returns>True if internal array was larger than max capacity -- a trim occurred. Else false</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TrimExcess(int maxCapacity)
        {
            var oldArr = _items;
            
            if (maxCapacity < oldArr.Length)
            {
                _count = maxCapacity;

                _items = GC.AllocateUninitializedArray<T>(maxCapacity);
                
                oldArr.AsSpan(0, maxCapacity).CopyTo(_items);
                
                //Note that we don't have to zero anything, since we are trimming
            }

            return false;
        }

        /// <summary>Decrements Count. That's it.</summary>
        public void UnsafeRemoveLast() => _count--;
        /// <summary>Reduces Count by n. That's it.</summary>
        public void UnsafeRemoveLastN(int n) => _count -= n;

        public void UnsafeRemoveFirstN(int n)
        {
            var arr = _items;

            //[0, 1, 2]
            var indexOfMoveStart = n;
            
            var moveCount = _count - indexOfMoveStart;

            arr.AsSpan(indexOfMoveStart, moveCount).CopyTo(arr.AsSpan());

            _count -= n;
        }

        /// <summary>Removes by copying last element to index position. Does not retain order.</summary>
        public void UnsafeRemoveBySwap(int idx)
        {
            ref var first = ref MemoryMarshal.GetArrayDataReference(_items);

            ref var removed = ref Unsafe.Add(ref first, idx);
            
            var last = Unsafe.Add(ref first, --_count);

            removed = last;
        }

        public void UnsafeRemove_RetainingOrder(int idx)
        {
            var newCount = --_count;
            
            if (newCount != idx)
            {
                //[0, 1, 2]
                var moveCount = newCount - idx;

                var origin = _items.AsSpan(idx + 1, moveCount);

                var dest = _items.AsSpan(idx, moveCount);
                
                origin.CopyTo(dest);
            }
        }

        /// <summary>
        /// Removes the first matching element by copying the last element to the matched position.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns> 
        public int UnsafeRemoveFirstMatch(RefPredicate<T> predicate)
        {
            return UnsafeRemoveFirstMatch(new RefPredicateDelWrapper(predicate));
        }
        
        public interface IRefPredicate
        {
            public bool Match(ref T Item);
        }
        
        private readonly struct RefPredicateDelWrapper: IRefPredicate
        {
            private readonly RefPredicate<T> Del;

            public RefPredicateDelWrapper(RefPredicate<T> del)
            {
                Del = del;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public bool Match(ref T Item)
            {
                return Del(ref Item);
            }
        }
        
        /// <summary>
        /// Removes the first matching element by copying the last element to the matched position.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns> 
        public int UnsafeRemoveFirstMatch<PredicateT>(PredicateT predicate)
            where PredicateT: IRefPredicate
        {
            ref var first = ref MemoryMarshal.GetArrayDataReference(_items);

            ref var current = ref first;
            
            ref var lastOffsetByOne = ref Unsafe.Add(ref current, Count--);

            for (; !Unsafe.AreSame(ref current, ref lastOffsetByOne); current = ref Unsafe.Add(ref current, 1))
            {
                if (!predicate.Match(ref current))
                {
                    continue;
                }

                current = Unsafe.Subtract(ref lastOffsetByOne, 1);

                return (int) Unsafe.ByteOffset(ref first, ref current) / Unsafe.SizeOf<T>();
            }

            return -1;
        }

        /// <summary>
        /// Removes the first matching element by shifting all remaining elements one position toward array start (maintains order).
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>The index of the removed match or -1 if no match was found</returns>
        public int UnsafeRemoveFirstMatch_RetainingOrder(RefPredicate<T> predicate)
        {
            return UnsafeRemoveFirstMatch_RetainingOrder(new RefPredicateDelWrapper(predicate));
        }
        
        public int UnsafeRemoveFirstMatch_RetainingOrder<PredicateT>(PredicateT predicate)
            where PredicateT: IRefPredicate
        {
            ref var first = ref MemoryMarshal.GetArrayDataReference(_items);

            ref var current = ref first;
            
            ref var lastOffsetByOne = ref Unsafe.Add(ref current, Count--);

            for (; !Unsafe.AreSame(ref current, ref lastOffsetByOne); current = ref Unsafe.Add(ref current, 1))
            {
                if (!predicate.Match(ref current))
                {
                    continue;
                }

                var indexOfRemoved = (int) Unsafe.ByteOffset(ref first, ref current) / Unsafe.SizeOf<T>();

                UnsafeRemove_RetainingOrder(indexOfRemoved);
                
                return indexOfRemoved;
            }

            return -1;
        }

        public int FirstMatchIdx(Func<T, bool> condition)
        {
            for (int i = 0; i < _count; i++)
            {
                if (condition(_items[i]))
                    return i;
            }

            return -1;
        }

        public bool Any(Func<T, bool> condition)
        {
            for (int i = 0; i < _count; i++)
            {
                if (condition(_items[i]))
                    return true;
            }

            return false;
        }


        /// <summary>Avg Time: O(n log(n)). Worst: O(n^2).   Operates directly on the underlying array.  Java considers it to be the fastest option for 47 to 285 items. </summary>
        /// <param name="comparer"></param>
        public void QuickSort(RefGreaterThan<T> comparer)
        {
            //Here's a good explanation of quicksort:
            //https://lamfo-unb.github.io/2019/04/21/Sorting-algorithms/
            throw new NotImplementedException();
        }

        /// <summary>Avg Time: O(n^2). Worst: O(n^2).  Operates directly on the underlying array. Java considers it to be the fastest option for less than 47 items. </summary>
        /// <param name="greaterThan"></param>
        public void InsertionSort(RefGreaterThan<T> greaterThan)
        {
            for (int i = 1; i < _count; ++i)
            {
                ref T key = ref _items[i];
                int j = i - 1;

                // Move elements of arr[0..i-1] that are greater than key
                // to one position ahead of their current position
                while (j > -1 && greaterThan(ref _items[j], ref key))
                {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = key;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public RefList<T> DeepCopy()
        {
            var newList = new RefList<T>(Capacity, _defaultInitialize, _defaultValue)
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
