using System;
using System.Threading;

namespace Nito.AsyncEx
{
    /// <summary>
    /// Utility class for temporarily switching <see cref="SynchronizationContext"/> implementations.
    /// </summary>
    public sealed class SynchronizationContextSwitcher : Disposables.SingleDisposable<object>
    {
        /// <summary>
        /// The previous <see cref="SynchronizationContext"/>.
        /// </summary>
        private readonly SynchronizationContext _oldContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextSwitcher"/> class, installing the new <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="newContext">The new <see cref="SynchronizationContext"/>. This can be <c>null</c> to remove an existing <see cref="SynchronizationContext"/>.</param>
        public SynchronizationContextSwitcher(SynchronizationContext newContext)
            : base(new object())
        {
            _oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        /// <summary>
        /// Restores the old <see cref="SynchronizationContext"/>.
        /// </summary>
        protected override void Dispose(object context)
        {
            SynchronizationContext.SetSynchronizationContext(_oldContext);
        }

        /// <summary>
        /// Removes the current <see cref="SynchronizationContext"/> and restores it when the returned disposable is disposed.
        /// </summary>
        public static IDisposable NoContext()
        {
            return new SynchronizationContextSwitcher(null);
        }
    }
}
