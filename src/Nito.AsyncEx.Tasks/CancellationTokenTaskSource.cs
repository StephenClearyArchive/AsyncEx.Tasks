using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Holds the task for a cancellation token, as well as the token registration. The registration is disposed when this instance is disposed.
    /// </summary>
    public sealed class CancellationTokenTaskSource : IDisposable
    {
        /// <summary>
        /// The cancellation token registration, if any. This is <c>null</c> if the registration was not necessary.
        /// </summary>
        private readonly IDisposable _registration;

        /// <summary>
        /// Creates a task for the specified cancellation token, registering with the token if necessary.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = Task.FromCanceled(cancellationToken);
                return;
            }
            var tcs = new TaskCompletionSource<object>();
            _registration = cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            Task = tcs.Task;
        }

        /// <summary>
        /// Gets the task for the source cancellation token.
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// Disposes the cancellation token registration, if any. Note that this may cause <see cref="Task"/> to never complete.
        /// </summary>
        public void Dispose()
        {
            if (_registration != null)
                _registration.Dispose();
        }

        /// <summary>
        /// Asynchronously waits for the task to complete or for the cancellation token to be canceled.
        /// It is not possible to distinguish between a canceled task completing and the cancellation token cancelling the wait, unless the cancellation tokens are distinguishable.
        /// </summary>
        /// <param name="waitTask">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public static Task WaitAsync(Task waitTask, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return waitTask;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);
            return DoWaitAsync(waitTask, cancellationToken);
        }

        private static async Task DoWaitAsync(Task waitTask, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource(cancellationToken))
                await await Task.WhenAny(waitTask, cancelTaskSource.Task).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits for the task to complete or for the cancellation token to be canceled.
        /// It is not possible to distinguish between a canceled task completing and the cancellation token cancelling the wait, unless the cancellation tokens are distinguishable.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="waitTask">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public static Task<TResult> WaitAsync<TResult>(Task<TResult> waitTask, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
                return waitTask;
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResult>(cancellationToken);
            return DoWaitAsync(waitTask, cancellationToken);
        }

        private static async Task<TResult> DoWaitAsync<TResult>(Task<TResult> waitTask, CancellationToken cancellationToken)
        {
            using (var cancelTaskSource = new CancellationTokenTaskSource(cancellationToken))
            {
                var completedTask = await Task.WhenAny(waitTask, cancelTaskSource.Task).ConfigureAwait(false);
                if (completedTask == waitTask)
                    return await waitTask; // No ConfigureAwait necessary because the task is already completed.
                cancellationToken.ThrowIfCancellationRequested();

                // Should never get here.
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}