using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Nito.Disposables;

namespace Nito.Mvvm
{
    /// <summary>
    /// A command that cancels a <see cref="CancellationToken"/> when it is executed. "Operations" may be started for this command. This command is canceled whenever there are no operations.
    /// </summary>
    public sealed class CancelCommand : ICommand
    {
        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        private readonly WeakCanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// The cancellation token source currently controlled by this command.
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// The number of current operations. If this is <c>0</c>, then the command is canceled.
        /// </summary>
        private int _count;

        /// <summary>
        /// Creates a new cancel command.
        /// </summary>
        public CancelCommand()
        {
            _cts = new CancellationTokenSource();
            _cts.Cancel();
            _canExecuteChanged = new WeakCanExecuteChanged(this);
        }

        /// <summary>
        /// Gets a cancellation token that will be canceled when this command is executed.
        /// </summary>
        public CancellationToken CancellationToken => _cts.Token;

        /// <summary>
        /// Gets a value indicating whether the cancel command has been executed.
        /// </summary>
        public bool IsCancellationRequested => _cts.IsCancellationRequested;

        /// <summary>
        /// Executes the cancel command.
        /// </summary>
        private void Cancel()
        {
            _cts.Cancel();
            _canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Resets this cancel command to an uncanceled state.
        /// </summary>
        private void Reset()
        {
            _cts = new CancellationTokenSource();
            _canExecuteChanged.OnCanExecuteChanged();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { _canExecuteChanged.CanExecuteChanged += value; }
            remove { _canExecuteChanged.CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return !IsCancellationRequested;
        }

        void ICommand.Execute(object parameter)
        {
            Cancel();
        }

        /// <summary>
        /// Decrements the count, and cancels the command if the new count is <c>0</c>.
        /// </summary>
        private void Signal()
        {
            if (--_count == 0)
                Cancel();
        }

        /// <summary>
        /// Increments the count, and resets the command if the old count was <c>0</c>.
        /// </summary>
        private void AddRef()
        {
            if (_count++ == 0)
                Reset();
        }

        /// <summary>
        /// Associates an operation with the cancelable command. The operation is disassociated when disposed.
        /// </summary>
        public IDisposable StartOperation()
        {
            return new SignalOnDispose(this);
        }

        /// <summary>
        /// Wraps a delegate so that it registers with this cancel command. The delegate is passed the <see cref="CancellationToken"/> of this cancel command. Any <see cref="OperationCanceledException"/> exceptions raised by the delegate are silently ignored.
        /// </summary>
        /// <param name="executeAsync">The cancelable delegate.</param>
        public Func<object, Task> WrapDelegate(Func<object, CancellationToken, Task> executeAsync)
        {
            return async parameter =>
            {
                using (StartOperation())
                {
                    try
                    {
                        await executeAsync(parameter, CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // We cannot use `when (ex.CancellationToken == cancellationToken)` on the catch block because the user delegate may be cancelled by a linked CancellationToken.
                    }
                }
            };
        }

        /// <summary>
        /// Wraps a delegate so that it registers with this cancel command. The delegate is passed the <see cref="CancellationToken"/> of this cancel command. Any <see cref="OperationCanceledException"/> exceptions raised by the delegate are silently ignored.
        /// </summary>
        /// <param name="executeAsync">The cancelable delegate.</param>
        public Func<Task> WrapDelegate(Func<CancellationToken, Task> executeAsync)
        {
            var wrapped = WrapDelegate((_, token) => executeAsync(token));
            return () => wrapped(null);
        }

        private sealed class SignalOnDispose : SingleDisposable<CancelCommand>
        {
            public SignalOnDispose(CancelCommand context) : base(context)
            {
                context.AddRef();
            }

            protected override void Dispose(CancelCommand context)
            {
                context.Signal();
            }
        }
    }
}