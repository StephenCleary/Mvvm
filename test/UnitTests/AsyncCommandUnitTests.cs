using Nito.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace UnitTests
{
    public class AsyncCommandUnitTests
    {
        [Fact]
        public void AfterConstruction_IsNotExecuting()
        {
            var command = new AsyncCommand(_ => Task.FromResult(0));
            Assert.False(command.IsExecuting);
            Assert.Null(command.Execution);
            Assert.True(((ICommand)command).CanExecute(null));
        }

        [Fact]
        public void AfterSynchronousExecutionComplete_IsNotExecuting()
        {
            var command = new AsyncCommand(_ => Task.FromResult(0));

            ((ICommand)command).Execute(null);

            Assert.False(command.IsExecuting);
            Assert.NotNull(command.Execution);
            Assert.True(((ICommand)command).CanExecute(null));
        }

        [Fact]
        public void StartExecution_IsExecuting()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new AsyncCommand(_ => signal.Task);

            ((ICommand)command).Execute(null);

            Assert.True(command.IsExecuting);
            Assert.NotNull(command.Execution);
            Assert.False(((ICommand)command).CanExecute(null));

            signal.SetResult(null);
        }

        [Fact]
        public void StartExecution_NotifiesPropertyChanges()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new AsyncCommand(_ => signal.Task);
            var isExecutingNotification = TestUtils.PropertyNotified(command, n => n.IsExecuting);
            var executionNotification = TestUtils.PropertyNotified(command, n => n.Execution);
            var sawCanExecuteChanged = false;
            ((ICommand)command).CanExecuteChanged += (_, __) => { sawCanExecuteChanged = true; };

            ((ICommand)command).Execute(null);

            Assert.True(isExecutingNotification());
            Assert.True(executionNotification());
            Assert.True(sawCanExecuteChanged);

            signal.SetResult(null);
        }
    }
}