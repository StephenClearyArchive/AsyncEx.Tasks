using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.Async
{
    // TODO: determine if we can remove the "threadsafe" restriction, and always treat these as under lock.

    /// <summary>
    /// A collection of cancelable <see cref="TaskCompletionSource{T}"/> instances. Implementations must be threadsafe <b>and</b> must work correctly if the caller is holding a lock.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
    public interface IAsyncWaitQueue<T>
    {
        /// <summary>
        /// Gets whether the queue is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. The returned task must support both synchronous and asynchronous waits.
        /// </summary>
        /// <returns>The queued task.</returns>
        Task<T> Enqueue();

        /// <summary>
        /// Removes a single entry in the wait queue and completes it.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entry. If this isn't needed, use <c>default(T)</c>.</param>
        void Dequeue(T result = default(T));

        /// <summary>
        /// Removes all entries in the wait queue and completes them.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entries. If this isn't needed, use <c>default(T)</c>.</param>
        void DequeueAll(T result = default(T));

        /// <summary>
        /// Attempts to remove an entry from the wait queue and cancels it.
        /// </summary>
        /// <param name="task">The task to cancel.</param>
        /// <param name="cancellationToken">The cancellation token to use to cancel the task.</param>
        void TryCancel(Task task, CancellationToken cancellationToken);

        /// <summary>
        /// Removes all entries from the wait queue and cancels them.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use to cancel the tasks.</param>
        void CancelAll(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides extension methods for wait queues.
    /// </summary>
    public static class AsyncWaitQueueExtensions
    {
        /// <summary>
        /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
        /// </summary>
        /// <param name="this">The wait queue.</param>
        /// <param name="syncObject">A synchronization object taken while cancelling the entry.</param>
        /// <param name="token">The token used to cancel the wait.</param>
        /// <returns>The queued task.</returns>
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, object syncObject, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return TaskConstants<T>.Canceled;

            var ret = @this.Enqueue();
            if (!token.CanBeCanceled)
                return ret;

            var registration = token.Register(() =>
            {
                lock (syncObject)
                    @this.TryCancel(ret, token);
            }, useSynchronizationContext: false);
            ret.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }
    }

    /// <summary>
    /// The default wait queue implementation, which uses a double-ended queue.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DefaultAsyncWaitQueue<>.DebugView))]
    public sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T>
    {
        private readonly Deque<AsyncTaskSource<T>> _queue = new Deque<AsyncTaskSource<T>>();

        private int Count
        {
            get { lock (_queue) { return _queue.Count; } }
        }

        bool IAsyncWaitQueue<T>.IsEmpty
        {
            get { return Count == 0; }
        }

        Task<T> IAsyncWaitQueue<T>.Enqueue()
        {
            var tcs = new AsyncTaskSource<T>();
            lock (_queue)
                _queue.AddToBack(tcs);
            return tcs.Task;
        }

        void IAsyncWaitQueue<T>.Dequeue(T result)
        {
            lock (_queue)
                _queue.RemoveFromFront().TrySetResult(result);
        }

        void IAsyncWaitQueue<T>.DequeueAll(T result)
        {
            lock (_queue)
            {
                foreach (var source in _queue)
                    source.TrySetResult(result);
                _queue.Clear();
            }
        }

        void IAsyncWaitQueue<T>.TryCancel(Task task, CancellationToken cancellationToken)
        {
            lock (_queue)
            {
                for (int i = 0; i != _queue.Count; ++i)
                {
                    if (_queue[i].Task == task)
                    {
                        _queue[i].TrySetCanceled(cancellationToken);
                        _queue.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        void IAsyncWaitQueue<T>.CancelAll(CancellationToken cancellationToken)
        {
            lock (_queue)
            {
                foreach (var source in _queue)
                    source.TrySetCanceled(cancellationToken);
                _queue.Clear();
            }
        }

        [DebuggerNonUserCode]
        internal sealed class DebugView
        {
            private readonly DefaultAsyncWaitQueue<T> _queue;

            public DebugView(DefaultAsyncWaitQueue<T> queue)
            {
                _queue = queue;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Task<T>[] Tasks
            {
                get
                {
                    var result = new List<Task<T>>(_queue._queue.Count);
                    foreach (var entry in _queue._queue)
                        result.Add(entry.Task);
                    return result.ToArray();
                }
            }
        }
    }
}
