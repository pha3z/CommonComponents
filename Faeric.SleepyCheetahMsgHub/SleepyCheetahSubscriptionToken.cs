using System;
using System.Collections.Generic;
using System.Text;

namespace Faeric.SleepyCheetahMsgHub
{
    public struct SleepyCheetahSubscriptionToken
    {
        /// <summary>
        /// The type of message for the subscription
        /// </summary>
        public readonly Type MsgType;

        public readonly ushort SubscriptionId;

        public SleepyCheetahSubscriptionToken(Type msgType, ushort subId)
        {
            MsgType = msgType;
            SubscriptionId = subId;
        }
    }
}
