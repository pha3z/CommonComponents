using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    /// <summary>
    /// https://stackoverflow.com/questions/22132995/extension-method-to-convert-flags-to-ienumerableenum-and-conversely-c/22132996
    /// </summary>
    public static class EnumExt
    {

        /// <summary>
        /// Turns an IEnumerable of values into a bitmask Flags enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T ToFlagsEnum<T>(this IEnumerable<T> values) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type.");

            int builtValue = 0;
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (values.Contains(value))
                {
                    builtValue |= Convert.ToInt32(value);
                }
            }
            return (T)Enum.Parse(typeof(T), builtValue.ToString());
        }

        /// <summary>
        /// Checks all the bits in a flags enum and returns an IEnumerable for all On bits
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToFlagsCollection<T>(this T flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type.");

            int inputInt = (int)(object)(T)flags;
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                int valueInt = (int)(object)(T)value;
                if (0 != (valueInt & inputInt))
                {
                    yield return value;
                }
            }
        }
    }
}
