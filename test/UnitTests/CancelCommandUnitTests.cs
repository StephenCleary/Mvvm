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
            Assert.False(((ICommand)command).CanExecute(null));
        }

        [Fact]
        public async Task Wrap_LeavesCanceledState()
        {
            var command = new CancelCommand();
            var observedCancellationToken = new CancellationToken();
            object observedSender = null;
            bool observedCanExecute = false;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            ((ICommand)command).CanExecuteChanged += subscription;
            var ready = new TaskCompletionSource<object>();
            var release = new TaskCompletionSource<object>();

            var task = command.WrapDelegate(async token =>
            {
                observedCancellationToken = token;
                ready.SetResult(null);
                await release.Task;
            })();
            await ready.Task;

            Assert.False(observedCancellationToken.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.True(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
            release.SetResult(null);
            await task;
        }

        [Fact]
        public async Task Execute_EntersCanceledState()
        {
            var command = new CancelCommand();
            var observedCancellationToken = new CancellationToken();
            object observedSender = null;
            bool observedCanExecute = false;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            var ready = new TaskCompletionSource<object>();
            var release = new TaskCompletionSource<object>();

            var task = command.WrapDelegate(async token =>
            {
                observedCancellationToken = token;
                ready.SetResult(null);
                await release.Task;
            })();
            await ready.Task;
            ((ICommand)command).CanExecuteChanged += subscription;

            ((ICommand)command).Execute(null);

            Assert.True(observedCancellationToken.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
            release.SetResult(null);
            await task;
        }

        [Fact]
        public async Task WrappedDelegateCompleted_EntersCanceledState()
        {
            var command = new CancelCommand();
            var observedCancellationToken = new CancellationToken();
            object observedSender = null;
            bool observedCanExecute = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            var ready = new TaskCompletionSource<object>();
            var release = new TaskCompletionSource<object>();

            var task = command.WrapDelegate(async token =>
            {
                observedCancellationToken = token;
                ready.SetResult(null);
                await release.Task;
            })();
            await ready.Task;
            ((ICommand)command).CanExecuteChanged += subscription;

            release.SetResult(null);
            await task;

            Assert.True(observedCancellationToken.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public async Task Operations_AreReferenceCounted()
        {
            var command = new CancelCommand();
            var observedCancellationToken1 = new CancellationToken();
            var ready1 = new TaskCompletionSource<object>();
            var release1 = new TaskCompletionSource<object>();
            var observedCancellationToken2 = new CancellationToken();
            var ready2 = new TaskCompletionSource<object>();
            var release2 = new TaskCompletionSource<object>();

            var task1 = command.WrapDelegate(async token =>
            {
                observedCancellationToken1 = token;
                ready1.SetResult(null);
                await release1.Task;
            })();
            var task2 = command.WrapDelegate(async token =>
            {
                observedCancellationToken2 = token;
                ready2.SetResult(null);
                await release2.Task;
            })();
            await Task.WhenAll(ready1.Task, ready2.Task);

            release2.SetResult(null);
            await task2;

            Assert.True(observedCancellationToken1 == observedCancellationToken2);
            Assert.False(observedCancellationToken1.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));

            release1.SetResult(null);
            await task1;

            Assert.True(observedCancellationToken1.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
        }
    }
}