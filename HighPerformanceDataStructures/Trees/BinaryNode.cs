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

        //public List<ushort> Children; //This makes us deference to get children.
                                      //This is undesirable, but possibly unavoidable.
                                      //Unless we went to binary tree.
                                      //It makes for a double memory jump to get to a child.. which would be bad in cases of just one child.
                                      //In those cases, an ordinary reference would be better.
                                      //
                                      //Maybe we should just go to a regular class node tree implementation
                                      //we can snag one from github
                                      //Then we don't have to do any of this.
                                      //Could we work with a binary implementation?
                                      //That seems like it would make the logic more complicated
                                      //But it would eliminate jumps to other data structures

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