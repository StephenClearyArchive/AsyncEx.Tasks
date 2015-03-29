using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using Nito.AsyncEx.Testing;
using Xunit;

namespace UnitTests
{
    public class CancellationTokenTaskSourceUnitTests
    {
        [Fact]
        public void WaitAsync_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = CancellationTokenTaskSource.WaitAsync(tcs.Task, CancellationToken.None);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsync_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = CancellationTokenTaskSource.WaitAsync(tcs.Task, token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public async Task WaitAsync_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = CancellationTokenTaskSource.WaitAsync(tcs.Task, cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }
    }
}
