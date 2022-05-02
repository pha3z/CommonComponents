using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Faeric.HighPerformanceDataStructures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LtNode<T> where T : struct
    {
        public short Principal;
        public short Child;
        public short StackSibling;
        public short OverlaySibling;

        public T Value;

        public LtNode(short principal, ref T value)
        {
            Principal = principal;
            Child = LtTree<T>.EMPTY_REF;
            OverlaySibling = LtTree<T>.EMPTY_REF;
            StackSibling = LtTree<T>.EMPTY_REF;
            Value = value;
        }

        public LtNode(short principal)
        {
            Principal = principal;
            Child = LtTree<T>.EMPTY_REF;
            OverlaySibling = LtTree<T>.EMPTY_REF;
            StackSibling = LtTree<T>.EMPTY_REF;
            Value = default;
        }


    }
}