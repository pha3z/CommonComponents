using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static System.Linq.Enumerable;

namespace Common
{
    public static class Numerics
    {
        public static IEnumerator<int> GetEnumerator(this Range range)
        {

            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                yield return i;
            }
        }
    }
}
