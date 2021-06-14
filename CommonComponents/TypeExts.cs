using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common
{
    public static class TypeExts
    {
        public static bool Is<TCase>(this Type o)
        {
            return typeof(TCase).IsAssignableFrom(o);
        }

        public static bool Is<TCase>(this object o)
        {
            return typeof(TCase).IsAssignableFrom(o.GetType());
        }
    }
}
