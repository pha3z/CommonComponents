using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Numerics
    {
        public static int[] Sequence(int start, int count, int incrementBy = 1)
        {
            int[] numbers = new int[count];

            for (int i = 0; i < count; i++)
                numbers[i] = (incrementBy * i) + start;

            return numbers;
        }

    }
}
