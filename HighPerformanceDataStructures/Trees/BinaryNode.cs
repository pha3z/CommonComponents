using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Faeric.HighPerformanceDataStructures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BinaryNode<T> where T : struct
    {
        /// <summary>Root node has index of 1. This allows index values of 0 to indicate an empty reference.</summary>
        public short Parent;
        /// <summary>Root node has index of 1. This allows index values of 0 to indicate an empty reference.</summary>
        public short Left;
        /// <summary>Root node has index of 1. This allows index values of 0 to indicate an empty reference.</summary>
        public short Right;
        public T Value;

        public BinaryNode(short parent, ref T value)
        {
            Parent = parent;
            Left = 0;
            Right = 0;
            Value = value;
        }

        public BinaryNode(short parent)
        {
            Parent = parent;
            Left = 0;
            Right = 0;
            Value = default;
        }

        
    }
}