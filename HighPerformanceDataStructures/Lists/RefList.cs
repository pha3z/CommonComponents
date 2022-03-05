using System;
using System.Collections.Generic;
using System.Text;

namespace Faeric.HighPerformanceDataStructures
{
    public  class RefList
    {
        public int Count { get => _count; set => _count = value; }
        protected int _count;

        public void Clear() => _count = 0;
    }
}
