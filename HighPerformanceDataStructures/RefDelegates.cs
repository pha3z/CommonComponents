using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public delegate bool RefPredicate<T>(ref T item);
    public delegate bool RefGreaterThan<T>(ref T item1, ref T item2);
}
