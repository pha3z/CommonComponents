using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Faeric.SleepyCheetahMsgHub
{

    internal struct SleepyCheetahSubscription
    {
        public ushort SubId;
        public Action NotifyMessagesAreAvailable;
    }
}
