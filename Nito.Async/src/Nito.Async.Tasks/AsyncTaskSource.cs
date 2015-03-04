using Nito.Async.Synchronous;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.Async
{
    /// <summary>
    /// A task completion source that forces task continuations to execute asynchronously. All members are fully threadsafe.
    /// </summary>
    public sealed class AsyncTaskSource<T>
    {
        /// <summary>
        /// The underlying task completion source.
        /// </summary>
        private readonly TaskCompletionSource<T> _tcs;

        /// <summary>
        /// Creates a new asynchronous task source.
        /// </summary>
        public AsyncTaskSource()
        {
            _tcs = new TaskCompletionSource<T>(TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Gets the task that is controlled by this source.
        /// </summary>
        public Task<T> Task {  get { return _tcs.Task; } }

        /// <summary>
        /// Attempts to complete the task with the specified result value. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <param name="result">The result value for the task.</param>
        public bool TrySetResult(T result = default(T))
        {
            return _tcs.TrySetResult(result);
        }

        /// <summary>
        /// Attempts to complete the task as faulted with the specified exception. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <param name="exception">The exception for the task.</param>
        public bool TrySetException(Exception exception)
        {
            return _tcs.TrySetException(exception);
        }

        /// <summary>
        /// Attempts to complete the task as faulted with the specified exceptions. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <param name="exception">The exceptions for the task.</param>
        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            return _tcs.TrySetException(exceptions);
        }

        /// <summary>
        /// Attempts to complete the task as canceled. Returns <c>false</c> if the task is already completed.
        /// </summary>
        public bool TrySetCanceled()
        {
            return _tcs.TrySetCanceled();
        }

        /// <summary>
        /// Attempts to complete the task as canceled with the specified cancellation token. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
            return _tcs.TrySetCanceled(cancellationToken);
        }

        /// <summary>
        /// Attempts to complete the task, propagating the completion of <paramref name="task"/>. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <typeparam name="TSourceResult">The type of the result of the source asynchronous operation.</typeparam>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        public bool TryCompleteFromCompletedTask<TSourceResult>(Task<TSourceResult> task) where TSourceResult : T
        {
            if (task.IsFaulted)
                return _tcs.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException exception)
                {
                    var token = exception.CancellationToken;
                    if (!token.IsCancellationRequested)
                        token = new CancellationToken(true);
                    return _tcs.TrySetCanceled(token);
                }
            }
            return _tcs.TrySetResult(task.Result);
        }

        /// <summary>
        /// Attempts to complete the task, propagating the completion of <paramref name="task"/>. Returns <c>false</c> if the task is already completed.
        /// </summary>
        /// <typeparam name="TSourceResult">The type of the result of the source asynchronous operation.</typeparam>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        /// <param name="result">The result value for the task.</param>
        public bool TryCompleteFromCompletedTask(Task task, T result)
        {
            if (task.IsFaulted)
                return _tcs.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
            {
                try
                {
                    task.WaitAndUnwrapException();
                }
                catch (OperationCanceledException exception)
                {
                    var token = exception.CancellationToken;
                    if (!token.IsCancellationRequested)
                        token = new CancellationToken(true);
                    return _tcs.TrySetCanceled(token);
                }
            }
            return _tcs.TrySetResult(result);
        }
    }
}