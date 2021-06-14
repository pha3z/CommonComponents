using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    /// <summary>
    /// Producer/Consumer with a twist
    /// Producer provides objects with identifiers (best if the identifier comes from an internal property or field on the object)
    /// Tracker will ignore object that it's already received by comparing identifiers (this is how the Queue gets named Exclusive. Duplicates are rejected)
    /// Consumer can pop objects and know it will only receive unique objects (presuming the provider has honored the identifier usage correctly)
    /// 
    /// To achieve the goal, objects are retained internally for a configurable length of time
    /// </summary>
    public class ExclusiveQueue<T>
    {
        private class ObjectRecord
        {
            public string TrackedObjectIdentifier;
            public DateTime BeganTracking;
        }

        //All events get added to the dictionary
        //Events that have been in the dictionary for more than N minutes are removed
        //By tracking time-in-dictionary instead of event timestamp, we decouple the event database time from machine time on which
        //this application runs
        private ConcurrentDictionary<string, T> _objects = new ConcurrentDictionary<string, T>();
        private ConcurrentQueue<ObjectRecord> _rememberedObjects = new ConcurrentQueue<ObjectRecord>();

        /// <summary>
        /// These are the events which have not been popped by consumer
        /// </summary>
        private ConcurrentQueue<T> _unprocessedObjects = new ConcurrentQueue<T>();

        readonly int _retainEventsForNMinutes;

        public ExclusiveQueue(int retainEventsForNMinutes)
        {
            _retainEventsForNMinutes = retainEventsForNMinutes;
        }

        public void Add(T obj, string identifier)
        {
            if (!_objects.ContainsKey(identifier))
            {
                _objects[identifier] = obj;
                _unprocessedObjects.Enqueue(obj);
                _rememberedObjects.Enqueue(
                    new ObjectRecord()
                    {
                        TrackedObjectIdentifier = identifier,
                        BeganTracking = DateTime.UtcNow
                    });
            }
        }

        public void Add(IEnumerable<Tuple<T, string>> objectAndIdentifier)
        {
            foreach (var t in objectAndIdentifier)
                Add(t.Item1, t.Item2);
        }

        /// <summary>
        /// Pops and returns the next events from the internal queue.  If count is unspecified (default 0), all available events are popped and returned.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<T> Pop(int count = 0)
        {
            List<T> objects = new List<T>(_unprocessedObjects.Count);
            T obj;

            int numToPop = (count == 0) ? _unprocessedObjects.Count : count;

            for (int i = 0; i < numToPop; i++)
            {
                if (!_unprocessedObjects.TryDequeue(out obj))
                {
                    Console.WriteLine("SmartObjectTracker error occurred. Reached end of queue unexpectedly.");
                    break; //If we hit the end of queue early for some reason, break. Although this should never happen.
                }

                objects.Add(obj);
            }

            RemoveStaleRecords();

            return objects;
        }

        private void RemoveStaleRecords()
        {
            ObjectRecord r;

            while (_rememberedObjects.TryPeek(out r))
            {
                if (DateTime.UtcNow.Subtract(r.BeganTracking).TotalMinutes > _retainEventsForNMinutes)
                {
                    _rememberedObjects.TryDequeue(out _);
                    _objects.TryRemove(r.TrackedObjectIdentifier, out _);
                }
                else
                    break; //terminate the loop if the next record is within retained timeframe
            }
        }
    }
}
