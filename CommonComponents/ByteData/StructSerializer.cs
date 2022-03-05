using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ByteData
{
    /// <summary>
    /// https://stackoverflow.com/questions/6335153/casting-a-byte-array-to-a-managed-structure
    /// </summary>
    public static class StructSerializer
    {
        public static unsafe byte[] Serialize<T>(T value) where T : unmanaged
        {
            byte[] buffer = new byte[sizeof(T)];

            fixed (byte* bufferPtr = buffer)
            {
                Buffer.MemoryCopy(&value, bufferPtr, sizeof(T), sizeof(T));
            }

            return buffer;
        }

        public static unsafe T Deserialize<T>(byte[] buffer) where T : unmanaged
        {
            T result = new T();

            fixed (byte* bufferPtr = buffer)
            {
                Buffer.MemoryCopy(bufferPtr, &result, sizeof(T), sizeof(T));
            }

            return result;
        }
    }
}
