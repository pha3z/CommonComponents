using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Removes the item at position key from dictionary and returns it.
        /// </summary>
        /// <param name="key">They key to remove. Value stored at this key will be returned</param>
        /// <returns>The value stored at index key</returns>
        public static V RemoveAndReturn<K, V>(this Dictionary<K, V> dic, K key)
        {
            V value = dic[key];
            dic.Remove(key);
            return value;
        }
    }
}
