using Nito.Mvvm;
using System;
using System.Windows.Input;
using Xunit;

namespace UnitTests
{
    public class CancelCommandUnitTests
    {
        [Fact]
        public void AfterConstruction_IsNotCanceled()
        {
            var command = new CancelCommand();
            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);
            Assert.True(command.CancellationToken.CanBeCanceled);
            Assert.True(((ICommand)command).CanExecute(null));
        }

        [Fact]
        public void Canceled_EntersCanceledState()
        {
            var command = new CancelCommand();
            var token = command.CancellationToken;
            object observedSender = null;
            bool observedCanExecute = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };
            ((ICommand)command).CanExecuteChanged += subscription;

            command.Cancel();

            Assert.True(token.IsCancellationRequested);
            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public void Execute_EntersCanceledState()
        {
            var command = new CancelCommand();
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
        public void Reset_LeavesCanceledState()
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

            command.Cancel();
            ((ICommand)command).CanExecuteChanged += subscription;
            command.Reset();

            Assert.True(token.IsCancellationRequested);
            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));
            Assert.Same(command, observedSender);
            Assert.True(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public void Canceled_InCanceledState_IsNoop()
        {
            var command = new CancelCommand();
            var token = command.CancellationToken;
            object observedSender = null;
            bool observedCanExecute = true;
            EventHandler subscription = (s, _) =>
            {
                observedSender = s;
                observedCanExecute = ((ICommand)command).CanExecute(null);
            };

            command.Cancel();
            ((ICommand)command).CanExecuteChanged += subscription;
            command.Cancel();

            Assert.True(token.IsCancellationRequested);
            Assert.True(command.CancellationToken.IsCancellationRequested);
            Assert.True(command.IsCancellationRequested);
            Assert.False(((ICommand)command).CanExecute(null));
            Assert.Null(observedSender);
            Assert.True(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }

        [Fact]
        public void Reset_InResetState_IsNoop()
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

            command.Cancel();
            command.Reset();
            ((ICommand)command).CanExecuteChanged += subscription;
            command.Reset();

            Assert.True(token.IsCancellationRequested);
            Assert.False(command.CancellationToken.IsCancellationRequested);
            Assert.False(command.IsCancellationRequested);
            Assert.True(((ICommand)command).CanExecute(null));
            Assert.Null(observedSender);
            Assert.False(observedCanExecute);

            ((ICommand)command).CanExecuteChanged -= subscription;
        }
    }
}