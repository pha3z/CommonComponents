using Faeric.HighPerformanceDataStructures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Faeric.SleepyCheetahMsgHub
{
    /// <summary>
    /// This is a light-weight message hub to be used with struct messages (they can be empty structs if needed).
    /// It uses a Dictionary&lt;Type, RefList&lt;Subscription&gt;&gt; to track subscriptions and messages.
    /// <br/><br/>
    /// Its called SleepyCheetah because its a deferred variant of the CheetahMsgHub. In Cheetah, messages are delivered synchronously
    /// when they are published. That makes Cheetah blazing fast because no temporary storage is required.
    /// In contrast, SleepyCheetah relies on an internal mailbox to enqueue published messages (while it sleeps).
    /// Periodically, you will need to "wake up" the Cheetah telling it to deliver all enqueued messages.
    /// <br/><br/>
    /// Also, when a consumer enqueues a message, if there are no active subscriptions for the message type, the message is immediately discarded -- work avoided!
    /// <br/><br/>
    /// DEPENDENCIES: Faeric.HighPerformanceDataStructures IRefListT is used to reference value types by REF.
    /// </summary>
    public class SleepyCheetahMsgHub : ISleepyCheetahMsgHub
    {
        private struct SubsAndMessages
        {
            public bool HasSubscriptions;
            public RefList<SleepyCheetahSubscription> Subscriptions;
            public RefList MessageQueue;

            public static SubsAndMessages New<T>()
            {
                return new SubsAndMessages()
                {
                    HasSubscriptions = true,
                    Subscriptions = new RefList<SleepyCheetahSubscription>(12),
                    MessageQueue = new RefList<T>(4)
                };
            }

        }

        readonly bool _errorOnDuplicateSubscriptionId = false;
        readonly Action<Type, Exception> _onErrorCallback;
        readonly Dictionary<Type, SubsAndMessages> _subsByType = new Dictionary<Type, SubsAndMessages>();
        readonly HashSet<Type> _messageTypesWaitingToBePublished = new HashSet<Type>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorOnDuplicateSubscriptionId"></param>
        /// <param name="onErrorCallback">An action called if an unhandled exception occurs in any of the subscription delegates. First parameter is message object (possibly null) and second is the Exception. Use GetType() to get the message's type.</param>
        public SleepyCheetahMsgHub(bool errorOnDuplicateSubscriptionId, Action<object, Exception> onErrorCallback)
        {
            _errorOnDuplicateSubscriptionId = errorOnDuplicateSubscriptionId;
            _onErrorCallback = onErrorCallback;
        }

        public SleepyCheetahSubscriptionToken Subscribe<Tmsg>(Action fnNotifyOfAvailableMessages, ushort subId) where Tmsg : struct
        {
            Type msgType = typeof(Tmsg);

            SubsAndMessages subsAndMsgs;
            if (!_subsByType.TryGetValue(msgType, out subsAndMsgs))
            {
                subsAndMsgs = SubsAndMessages.New<Tmsg>();
                _subsByType.Add(msgType, subsAndMsgs);
            }

            var subs = subsAndMsgs.Subscriptions;
            int existingSubIndex = -1;
            for (int i = 0; i < subs.Count; i++)
            {
                if (subs[i].SubId == subId)
                    existingSubIndex = i;
            }

            if (existingSubIndex == -1)
            {
                ref SleepyCheetahSubscription sub = ref subs.AddByRef();
                sub.NotifyMessagesAreAvailable = fnNotifyOfAvailableMessages;
                sub.SubId = subId;
            }
            else
            {
                if (_errorOnDuplicateSubscriptionId)
                    throw new ISleepyCheetahMsgHubException($"Subscription already exists for '{typeof(Tmsg).Name}' with SubscriptionId '{subId}' ");
                else
                {
                    ref SleepyCheetahSubscription sub = ref subs[existingSubIndex];
                    sub.NotifyMessagesAreAvailable = fnNotifyOfAvailableMessages;
                    sub.SubId= subId;
                }
            }

            return new SleepyCheetahSubscriptionToken(msgType, subId);
        }

        public void Unsubscribe(SleepyCheetahSubscriptionToken token)
        {
            SubsAndMessages snms;
            if (!_subsByType.TryGetValue(token.MsgType, out snms))
                return;

            
            var subs = snms.Subscriptions;
            int subId = token.SubscriptionId;
            for (int i = 0; i < subs.Count; i++)
            {
                if (subs[i].SubId == subId)
                {
                    subs.RemoveBySwap(i);
                    snms.HasSubscriptions = subs.Count != 0;
                    return;
                }
            }
        }

        public void Enqueue<Tmsg>() where Tmsg : struct
        {
            Tmsg m = default;
            Enqueue(ref m);
        }

        public void Enqueue<Tmsg>(ref Tmsg msg) where Tmsg : struct
        {
            Type t = typeof(Tmsg);
            SubsAndMessages snms;
            if (!_subsByType.TryGetValue(t, out snms))
                return; //No subscriptions. Discard message.
            if (!snms.HasSubscriptions)
                return; //No subscriptions. Discard message.

            var queue = (RefList<Tmsg>)snms.MessageQueue;
            ref Tmsg m = ref queue.AddByRef();
            m = msg;

            _messageTypesWaitingToBePublished.Add(t);
        }

        public IReadOnlyRefList<T> GetPendingMessagesReader<T>()
        {
            return (IReadOnlyRefList<T>)(_subsByType[typeof(T)].MessageQueue);
        }

        public void WakeUpAndPublishAllMessages()
        {
            foreach(Type t in _messageTypesWaitingToBePublished)
            {
                var snms = _subsByType[t];
                for (int i = 0; i < snms.Subscriptions.Count; i++)
                {
                    try
                        {snms.Subscriptions[i].NotifyMessagesAreAvailable();}
                    catch (Exception ex)
                        { _onErrorCallback?.Invoke(t, ex); }
                }

                snms.MessageQueue.Clear();
            }

            _messageTypesWaitingToBePublished.Clear();

        }
    }
}
