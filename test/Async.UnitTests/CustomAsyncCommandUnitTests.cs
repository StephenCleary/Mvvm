using Nito.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace UnitTests
{
    public class CustomAsyncCommandUnitTests
    {
        [Fact]
        public void AfterConstruction_IsNotExecuting()
        {
            var command = new CustomAsyncCommand(_ => Task.FromResult(0), _ => true);
            Assert.False(command.IsExecuting);
            Assert.Null(command.Execution);
        }

        [Fact]
        public void AfterSynchronousExecutionComplete_IsNotExecuting()
        {
            var command = new CustomAsyncCommand(_ => Task.FromResult(0), _ => true);

            ((ICommand)command).Execute(null);

            Assert.False(command.IsExecuting);
            Assert.NotNull(command.Execution);
        }

        [Fact]
        public void StartExecution_IsExecuting()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new CustomAsyncCommand(_ => signal.Task, _ => true);

            ((ICommand)command).Execute(null);

            Assert.True(command.IsExecuting);
            Assert.NotNull(command.Execution);

            signal.SetResult(null);
        }

        [Fact]
        public void Execute_DelaysExecutionUntilCommandIsInExecutingState()
        {
            bool isExecuting = false;
            NotifyTask execution = null;

            CustomAsyncCommand command = null;
            command = new CustomAsyncCommand(() =>
            {
                isExecuting = command.IsExecuting;
                execution = command.Execution;
                return Task.FromResult(0);
            }, () => true);

            ((ICommand)command).Execute(null);

            Assert.True(isExecuting);
            Assert.NotNull(execution);
        }

        [Fact]
        public void StartExecution_NotifiesPropertyChanges()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new CustomAsyncCommand(_ => signal.Task, _ => true);
            var isExecutingNotification = TestUtils.PropertyNotified(command, n => n.IsExecuting);
            var executionNotification = TestUtils.PropertyNotified(command, n => n.Execution);

            ((ICommand)command).Execute(null);

            Assert.True(isExecutingNotification());
            Assert.True(executionNotification());

            signal.SetResult(null);
        }
    }
}