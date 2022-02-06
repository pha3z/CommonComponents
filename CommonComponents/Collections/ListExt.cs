using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.Collections
{
    public static class ListExt
    {
        static class ArrayAccessor<T>
        {
            internal static FieldInfo internalArrayField;

            static ArrayAccessor()
            {
                internalArrayField = typeof(List<T>).GetField("_items",
               System.Reflection.BindingFlags.Instance |
               System.Reflection.BindingFlags.NonPublic);
            }
        }

        public static T[] GetInternalArray<T>(this List<T> list)
        {
            return (T[])ArrayAccessor<T>.internalArrayField.GetValue(list);
        }

        /// <summary>
        /// Removes and returns the element at position index without maintaining internal order. The last element will be moved to the position of the removed element.
        /// </summary>
        /// <param name="index">position of element to be removed. Last element will be moved to this position.</param>
        /// <returns></returns>
        public static T FastRemove<T>(this List<T> list, int index)
        {
            T val = list[index];
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return val;
        }


    }
}
