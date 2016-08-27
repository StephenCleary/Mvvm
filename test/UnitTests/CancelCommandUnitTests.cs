using Nito.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace UnitTests
{
    public class CancelCommandUnitTests
    {
        [Fact]
        public void AfterConstruction_IsCanceled()
        {
            var command = new CancelCommand();
            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
        }

        [Fact]
        public void StartOperation_LeavesCanceledState()
        {
            var command = new CancelCommand();
            var token = command.CancellationToken;
            object observedSender = null;
            bool observedCanExecute = false;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            ((ICommand)command).CanExecuteChanged += subscription;

            command.StartOperation();

            Assert.True(token.IsCancellationRequested);
            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.True(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public void Execute_EntersCanceledState()
        {
            var command = new CancelCommand();
            command.StartOperation();

            var token = command.CancellationToken;
            object observedSender = null;
            bool observedCanExecute = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            ((ICommand)command).CanExecuteChanged += subscription;

            ((ICommand)command).Execute(null);

            Assert.True(token.IsCancellationRequested);
            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public void StopOperation_EntersCanceledState()
        {
            var command = new CancelCommand();
            var operation = command.StartOperation();

            var token = command.CancellationToken;
            object observedSender = null;
            bool observedCanExecute = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            ((ICommand)command).CanExecuteChanged += subscription;

            operation.Dispose();

            Assert.True(token.IsCancellationRequested);
            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public async Task WrapDelegateCancellationToken_WhenCommandIsUncanceled_IsNotCanceled()
        {
            var command = new CancelCommand();
            var ctTcs = new TaskCompletionSource<CancellationToken>();
            var continueDelegate = new TaskCompletionSource<object>();
            var __ = command.WrapDelegate(async (_, token) =>
            {
                ctTcs.SetResult(token);
                await continueDelegate.Task;
            })(null);

            Assert.Equal(command.CancellationToken, await ctTcs.Task);
            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);

            continueDelegate.SetResult(null);

            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
        }

        [Fact]
        public void Operations_AreReferenceCounted()
        {
            var command = new CancelCommand();
            var operation1 = command.StartOperation();
            var operation2 = command.StartOperation();

            operation2.Dispose();

            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));

            operation1.Dispose();

            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
        }
    }
}