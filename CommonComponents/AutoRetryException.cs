using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class AutoRetryException : Exception
    {
        public AutoRetryException(string message)
            : base(message)
        {
        }

        public AutoRetryException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
