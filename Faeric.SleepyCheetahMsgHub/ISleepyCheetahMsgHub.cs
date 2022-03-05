using Faeric.HighPerformanceDataStructures;

namespace Faeric.SleepyCheetahMsgHub
{
    public interface ISleepyCheetahMsgHub
    {
        /// <summary>
        /// Enqueue an empty message unless there are no active subscribers for the message type. The message will be published when the cheetah wakes up.
        /// <br/>NOTE: An empty message consumes only 1 byte in the underlying msg array. This makes it reasonable performant to queue up a couple dozen empty messages.
        /// <br/>However if you are enqueuing hundreds or thousands of empty messages and need bleeding fast speed, you should create a specialized variant of sleepy Cheetah that internally tallies a count for empty messages instead of storing bytes in an array.
        /// </summary>
        /// <typeparam name="Tmsg"></typeparam>
        void Enqueue<Tmsg>() where Tmsg : struct;


        /// <summary>
        /// Enqueue a message unless there are no active subscribers for the message type. The message will be published when the cheetah wakes up.
        /// </summary>
        /// <typeparam name="Tmsg"></typeparam>
        /// <param name="msg">Msg is passed by ref in case you want to use jumbo structs. The msg will be copied into the internal array so there's still one copy. But if the msg parameter were passed by value, there would be two copies.</param>
        void Enqueue<Tmsg>(ref Tmsg msg) where Tmsg : struct;


        /// <summary>
        /// When subscriber is notified about pending messages, subscriber should invoke this method to get a reader for all pending messages of given type.
        /// <br/><br/>WARNING: The list is indexed by ref so you can read values without copying them.
        /// This design decision was made to reduce a lot of copying for structs with several/many fields.
        /// Be careful not to mutate the messages, or you will alter what other subscribers receive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyRefList<T> GetPendingMessagesReader<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Tmsg"></typeparam>
        /// <param name="action">An action that will be fired when there are one-or-more pending messages of the given TMsg type.<br/><br/>NOTE: DO NOT REGISTER THE SAME ACTION FOR MORE THAN ONE TYPE, because Action provider should call GetPendingMessagesReader() to read messages for the expected type. If you register the same action for more than one type, you won't know which type is pending when the action gets triggered.</param>
        /// <param name="subId">This only needs to be unique for subscriptions to Tmsg. It doesn't have to be unique for all subscriptions.</param>
        /// <returns>A token that can be used to Unsubscribe.</returns>
        SleepyCheetahSubscriptionToken Subscribe<Tmsg>(Action action, ushort subId) where Tmsg : struct;

        void Unsubscribe(SleepyCheetahSubscriptionToken token);
    }
}