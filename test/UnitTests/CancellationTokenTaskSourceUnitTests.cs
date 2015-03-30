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
        public void WaitAsyncTResult_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = tcs.Task.WaitAsync(CancellationToken.None);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsyncTResult_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = tcs.Task.WaitAsync(token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public async Task WaitAsyncTResult_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = tcs.Task.WaitAsync(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public void WaitAsync_TokenThatCannotCancel_ReturnsSourceTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var task = ((Task)tcs.Task).WaitAsync(CancellationToken.None);

            Assert.Same(tcs.Task, task);
        }

        [Fact]
        public void WaitAsync_AlreadyCanceledToken_ReturnsSynchronouslyCanceledTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var token = new CancellationToken(true);
            var task = ((Task)tcs.Task).WaitAsync(token);

            Assert.True(task.IsCanceled);
        }

        [Fact]
        public async Task WaitAsync_TokenCanceled_CancelsTask()
        {
            var tcs = new TaskCompletionSource<object>();
            var cts = new CancellationTokenSource();
            var task = ((Task)tcs.Task).WaitAsync(cts.Token);
            Assert.False(task.IsCompleted);

            cts.Cancel();

            await AsyncAssert.ThrowsAsync<OperationCanceledException>(task);
        }

        [Fact]
        public void Constructor_AlreadyCanceledToken_TaskReturnsSynchronouslyCanceledTask()
        {
            var token = new CancellationToken(true);
            using (var source = new CancellationTokenTaskSource<object>(token))
                Assert.True(source.Task.IsCanceled);
        }
    }
}
