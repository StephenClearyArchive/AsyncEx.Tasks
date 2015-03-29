using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnitTests
{
    public class TaskFactoryExtensionsUnitTests
    {
        [Fact]
        public async Task Run_SchedulesAction()
        {
            var factory = Task.Factory;
            TaskScheduler result = null;
            var task = factory.Run(() => { result = TaskScheduler.Current; });
            await task;
            Assert.Same(TaskScheduler.Default, result);
            Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task Run_SchedulesFunc()
        {
            var factory = Task.Factory;
            TaskScheduler result = null;
            var task = factory.Run(() => { result = TaskScheduler.Current; return 13; });
            await task;
            Assert.Same(TaskScheduler.Default, result);
            Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task Run_SchedulesAsyncAction()
        {
            var factory = Task.Factory;
            TaskScheduler result = null;
            var task = factory.Run(async () => { await Task.Delay(100); result = TaskScheduler.Current; });
            await task;
            Assert.Same(TaskScheduler.Default, result);
        }

        [Fact]
        public async Task Run_SchedulesAsyncFunc()
        {
            var factory = Task.Factory;
            TaskScheduler result = null;
            var task = factory.Run(async () => { await Task.Delay(100); result = TaskScheduler.Current; return 13; });
            await task;
            Assert.Same(TaskScheduler.Default, result);
        }
    }
}
