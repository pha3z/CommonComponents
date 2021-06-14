using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common
{
    public static class QueueExt
    {
        public static void EnqueueAll<T>(this Queue<T> q, IEnumerable<T> items)
        {
            foreach (var itm in items)
                q.Enqueue(itm);
        }

        /// <summary>
        /// Dequeues up to cnt items and returns them.  If cnt exceeds the size of the queue, the entire queue is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public static IEnumerable<T> Dequeue<T>(this Queue<T> q, int cnt)
        {
            List<T> items = new List<T>(cnt);

            for (int i = 0; i < cnt; i++)
            {
                items.Add(q.Dequeue());

                if (q.Count < 1)
                    break;
            }

            return items;
        }

        public static T DequeueOrNull<T>(this Queue<T> q)
            where T : class
        {
            if (q.Count < 1)
                return null;
            else
                return q.Dequeue();
        }

        public static IEnumerable<T> DequeueAll<T>(this Queue<T> q)
        {
            T[] items = new T[q.Count];

            for (int i = 0; i < q.Count; i++)
                items[i] = q.Dequeue();

            return items;
        }

        public static IEnumerable<T> Dequeue<T>(this ConcurrentQueue<T> q, int cnt)
        {
            T itm;

            List<T> items = new List<T>(cnt);

            for (int i = 0; i < cnt; i++)
            {
                if (q.TryDequeue(out itm))
                    items.Add(itm);

                if (q.Count < 1)
                    break;
            }

            return items;
        }

        public static T DequeueOrNull<T>(this ConcurrentQueue<T> q)
            where T : class
        {
            if (q.Count < 1)
                return null;
            else
            {
                T itm;
                return q.TryDequeue(out itm) ? itm : null;
            }
        }

        public static IEnumerable<T> DequeueAll<T>(this ConcurrentQueue<T> q)
        {
            List<T> items = new List<T>(q.Count);
            T itm;
            for (int i = 0; i < q.Count; i++)
            {
                if (q.TryDequeue(out itm))
                    items.Add(itm);
            }

            return items;
        }
    }
}
