using System.Threading;
using System.Threading.Tasks;

namespace Nito.Async
{
    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    public static class TaskConstants
    {
        private static readonly Task<bool> booleanTrue = Task.FromResult(true);
        private static readonly Task<int> intNegativeOne = Task.FromResult(-1);

        /// <summary>
        /// A task that has been completed with the value <c>true</c>.
        /// </summary>
        public static Task<bool> BooleanTrue
        {
            get
            {
                return booleanTrue;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>false</c>.
        /// </summary>
        public static Task<bool> BooleanFalse
        {
            get
            {
                return TaskConstants<bool>.Default;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>0</c>.
        /// </summary>
        public static Task<int> Int32Zero
        {
            get
            {
                return TaskConstants<int>.Default;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>-1</c>.
        /// </summary>
        public static Task<int> Int32NegativeOne
        {
            get
            {
                return intNegativeOne;
            }
        }

        /// <summary>
        /// A <see cref="Task"/> that has been completed.
        /// </summary>
        public static Task Completed
        {
            get
            {
#if ASPNET50
                return TaskConstants<object>.Default;
#else
                return Task.CompletedTask;
#endif
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task Canceled
        {
            get
            {
                return TaskConstants<object>.Canceled;
            }
        }
    }

    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    public static class TaskConstants<T>
    {
        private static readonly Task<T> defaultValue = Task.FromResult(default(T));

#if ASPNET50
        private static readonly Task<T> canceled = CreateCanceledTask();
        private static Task<T> CreateCanceledTask()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }
#else
        private static readonly Task<T> canceled = Task.FromCanceled<T>(new CancellationToken(true));
#endif

        /// <summary>
        /// A task that has been completed with the default value of <typeparamref name="T"/>.
        /// </summary>
        public static Task<T> Default
        {
            get
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task<T> Canceled
        {
            get
            {
                return canceled;
            }
        }
    }
}
