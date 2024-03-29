﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Faeric.SleepyCheetahMsgHub
{
    internal static class ListExt
    {
        /// <summary>
        /// Removes and returns the element at position index without maintaining internal order. The last element will be moved to the position of the removed element.
        /// </summary>
        /// <param name="index">position of element to be removed. Last element will be moved to this position.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T FastRemove<T>(this List<T> list, int index)
        {
            T val = list[index];
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return val;
        }


    }
}
