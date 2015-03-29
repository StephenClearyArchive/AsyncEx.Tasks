using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using Xunit;
using Nito.AsyncEx.Testing;

namespace UnitTests
{
    public class TaskCompletionSourceExtensionsUnitTests
    {
        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesResult()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
            var result = await tcs.Task;
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_WithDifferentTResult_PropagatesResult()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
            var result = await tcs.Task;
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesCancellation()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
            await AsyncAssert.ThrowsAsync<OperationCanceledException>(() => tcs.Task);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesException()
        {
            var source = new TaskCompletionSource<int>();
            source.TrySetException(new NotImplementedException());

            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(source.Task);
            await AsyncAssert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
        }
    }
}
