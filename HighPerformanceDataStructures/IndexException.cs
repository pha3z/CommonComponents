using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.HighPerformanceDataStructures
{
    public class IndexException : Exception
    {
        public IndexException(string message)
            : base(message)
        {
        }

        public IndexException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
