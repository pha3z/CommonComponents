using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    //There is an entire discussion on the topic of Retrying on Stackoverflow here:
    //https://stackoverflow.com/questions/1563191/cleanest-way-to-write-retry-logic
    //The base for this code came from that page.  Fabian Bigler provided the async example.

    //A comment on how to make this library more powerful:
    //TToni on StackOverflow said: We use a similar pattern for our DB access in a high volume Biztalk App,
    //but with two improvements: We have blacklists for exceptions that shouldn't be retried and we
    //store the first exception that occurs and throw that when the retry ultimately fails. Reason
    //being that the second and following exceptions often are different from the first one. In that
    //case you hide the initial problem when rethrowing only the last exception.
    //We throw a new exception with the original exception as inner exception.
    //The original stack trace is available as attribute from the inner exceptions

    public class Retry
    {

        //------ SYNCRHONOUS (THREAD-BLOCKING) METHODS FIRST--------------
        //------ Scroll down to Async portion of class -----------

        /// <summary>
        /// If retryIntervalMilliseconds is less than 1, the method will retry *immediately* without delay.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryIntervalMilliseconds"></param>
        /// <param name="maxAttemptCount"></param>
        public static void Do(Action action, int retryIntervalMilliseconds, int maxAttemptCount = 3)
        {
            Do(action, TimeSpan.FromMilliseconds(retryIntervalMilliseconds), maxAttemptCount);
        }

        /// <summary>
        /// If you want immediate retry without delay, call the overloaded method with Milliseconds and pass 0 milliseconds.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount">If less than 1, throws an exception</param>
        public static void Do(Action action, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        /// <summary>
        /// If retryIntervalMilliseconds is less than 1, the method will retry *immediately* without delay.  Throws an exception if all attempts fail.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryIntervalMilliseconds"></param>
        /// <param name="maxAttemptCount"></param>
        /// <returns></returns>
        public static T Do<T>(Func<T> action, int retryIntervalMilliseconds, int maxAttemptCount = 3)
        {
            return Do(action, TimeSpan.FromMilliseconds(retryIntervalMilliseconds), maxAttemptCount);
        }

        /// <summary>
        /// If you want immediate retry without delay, call the overloaded method with Milliseconds and pass 0 milliseconds. Throws exception if all attempts fail.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount">If less than 1, throws an exception</param>
        /// <returns></returns>
        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            //Don't remove this. We want consumers to know if they screwed up by passing a zero attempt count.
            if (maxAttemptCount == 0)
                throw new AutoRetryException("ERROR: Retry Attempt Counts must be greater than 0");

            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0 && retryInterval.Milliseconds > 1)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }


        //------ ASYNC (NON-BLOCKING) METHODS--------------

        /// <summary>
        /// If retryIntervalMilliseconds is less than 1, the method will retry *immediately* without delay.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="retryIntervalMilliseconds"></param>
        /// <param name="maxAttemptCount"></param>
        /// <returns></returns>
        public static async Task DoAsync(Func<Task> task, int retryIntervalMilliseconds, int maxAttemptCount = 3)
        {
            await DoAsync(task, TimeSpan.FromMilliseconds(retryIntervalMilliseconds), maxAttemptCount);
        }

        /// <summary>
        /// If you want immediate retry without delay, call the overloaded method with Milliseconds and pass 0 milliseconds.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount">If less than 1, throws an exception</param>
        /// <returns></returns>
        public static async Task DoAsync(Func<Task> task, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            //Don't remove this. We want consumers to know if they screwed up by passing a zero attempt count.
            if (maxAttemptCount == 0)
                throw new AutoRetryException("ERROR: Retry Attempt Counts must be greater than 0");

            var exceptions = new List<Exception>();
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0 && retryInterval.Milliseconds > 1)
                    {
                        await Task.Delay(retryInterval);
                    }

                    await task();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }

        /// <summary>
        /// If retryIntervalMilliseconds is less than 1, the method will retry *immediately* without delay.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="retryIntervalMilliseconds"></param>
        /// <param name="maxAttemptCount">If less than 1, throws an exception</param>
        /// <returns></returns>
        public static async Task<T> DoAsync<T>(Func<Task<T>> task, int retryIntervalMilliseconds, int maxAttemptCount = 3)
        {
            return await DoAsync(task, TimeSpan.FromMilliseconds(retryIntervalMilliseconds), maxAttemptCount);
        }

        /// <summary>
        /// If you want immediate retry without delay, call the overloaded method with Milliseconds and pass 0 milliseconds. If maxAttemptCount &lt; 1, throws exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount">If less than 1, throws an exception</param>
        /// <returns></returns>
        public static async Task<T> DoAsync<T>(Func<Task<T>> task, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            //Don't remove this. We want consumers to know if they screwed up by passing a zero attempt count.
            if (maxAttemptCount == 0)
                throw new AutoRetryException("ERROR: Retry Attempt Counts must be greater than 0");

            var exceptions = new List<Exception>();
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        await Task.Delay(retryInterval);
                    }
                    return await task();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }
    }


}
