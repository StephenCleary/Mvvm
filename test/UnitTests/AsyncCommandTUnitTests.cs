using Nito.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace UnitTests
{
    public class AsyncCommandTUnitTests
    {
        [Fact]
        public void AfterConstruction_IsNotExecuting()
        {
            var command = new AsyncCommand<string>(_ => Task.CompletedTask);
            Assert.False(command.IsExecuting);
            Assert.Null(command.Execution);
            Assert.True(((ICommand)command).CanExecute(""));
        }

        [Fact]
        public void AfterSynchronousExecutionComplete_IsNotExecuting()
        {
            var command = new AsyncCommand<int>(_ => Task.CompletedTask);

            ((ICommand)command).Execute(0);

            Assert.False(command.IsExecuting);
            Assert.NotNull(command.Execution);
            Assert.True(((ICommand)command).CanExecute(0));
        }

        [Fact]
        public async Task ExecuteWithWrongParameterType_ThrowsCastException()
        {
            var command = new AsyncCommand<bool>(_ => Task.CompletedTask);

            await Assert.ThrowsAsync<InvalidCastException>(() =>
                command.ExecuteAsync("A STRING")
            );
        }

        [Fact]
        public void StartExecution_IsExecuting()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new AsyncCommand<object>(_ => signal.Task);

            ((ICommand)command).Execute(null);

            Assert.True(command.IsExecuting);
            Assert.NotNull(command.Execution);
            Assert.False(((ICommand)command).CanExecute(null));

            signal.SetResult(null);
        }

        [Fact]
        public void Execute_DelaysExecutionUntilCommandIsInExecutingState()
        {
            bool isExecuting = false;
            NotifyTask execution = null;
            bool canExecute = true;

            AsyncCommand<int> command = null;
            command = new AsyncCommand<int>((intParam) =>
            {
                isExecuting = command.IsExecuting;
                execution = command.Execution;
                canExecute = ((ICommand)command).CanExecute(intParam);
                return Task.CompletedTask;
            });

            ((ICommand)command).Execute(0);

            Assert.True(isExecuting);
            Assert.NotNull(execution);
            Assert.False(canExecute);
        }

        [Fact]
        public void StartExecution_NotifiesPropertyChanges()
        {
            var signal = new TaskCompletionSource<object>();
            var command = new AsyncCommand<object>(_ => signal.Task);
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