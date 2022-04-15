using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    /// <summary>
    /// When you have a collection of stringly keys with a generic value attached to each one,
    /// you can express it more clearly using a collection of NameAndValue structs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct NameAndValue<T>
    {
        public string Name;
        public T Value;

        public NameAndValue(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
