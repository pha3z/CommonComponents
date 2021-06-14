using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public class AutoIndexException : Exception
    {
        public AutoIndexException(string message)
            : base(message)
        {
        }

        public AutoIndexException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
