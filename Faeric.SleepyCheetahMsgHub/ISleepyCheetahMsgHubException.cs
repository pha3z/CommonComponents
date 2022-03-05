using System;
using System.Collections.Generic;
using System.Text;

namespace Faeric.SleepyCheetahMsgHub
{
    public class ISleepyCheetahMsgHubException : SystemException
    {
        public ISleepyCheetahMsgHubException(string message)
    : base(message)
        {
        }

        public ISleepyCheetahMsgHubException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
