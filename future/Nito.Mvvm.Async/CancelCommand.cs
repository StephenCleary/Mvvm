using System;
using System.Reflection;
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
        private readonly ICanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// The cancellation token source currently controlled by this command. This is <c>null</c> when the current context has been cancelled.
        /// </summary>
        private RefCountedCancellationTokenSource? _context;

        /// <summary>
        /// Creates a new cancel command.
        /// </summary>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CancelCommand(Func<object, ICanExecuteChanged> canExecuteChangedFactory)
        {
            _ = canExecuteChangedFactory ?? throw new ArgumentNullException(nameof(canExecuteChangedFactory));
            _canExecuteChanged = canExecuteChangedFactory(this);
        }

        /// <summary>
        /// Creates a new cancel command.
        /// </summary>
        public CancelCommand()
            : this(CanExecuteChangedFactories.DefaultCanExecuteChangedFactory)
        {
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { _canExecuteChanged.CanExecuteChanged += value; }
            remove { _canExecuteChanged.CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return _context != null;
        }

        void ICommand.Execute(object parameter)
        {
            _context?.Cancel();
        }

        private IDisposable StartOperation()
        {
            if (_context == null)
            {
                _context = new RefCountedCancellationTokenSource(this);
                _canExecuteChanged.OnCanExecuteChanged();
            }
            return _context.StartOperation();
        }

        /// <summary>
        /// Called when the context has been cancelled.
        /// </summary>
        /// <param name="context">The context.</param>
        private void Notify(RefCountedCancellationTokenSource context)
        {
            if (_context != context)
                return;
            _context = null;
            _canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Cancels any current context.
        /// </summary>
        public void Cancel() => _context?.Cancel();

        /// <summary>
        /// Wraps a delegate so it cancels this cancel command and then registers with it. The delegate is passed the <see cref="CancellationToken"/> of this cancel command. Any <see cref="OperationCanceledException"/> exceptions raised by the delegate are silently ignored.
        /// </summary>
        public Func<object?, Task> WrapCancel(Func<object?, CancellationToken, Task> executeAsync)
        {
            var wrapped = Wrap(executeAsync);
            return async parameter =>
            {
                Cancel();
                await wrapped(parameter).ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Wraps a delegate so it cancels this cancel command and then registers with it. The delegate is passed the <see cref="CancellationToken"/> of this cancel command. Any <see cref="OperationCanceledException"/> exceptions raised by the delegate are silently ignored.
        /// </summary>
        public Func<Task> WrapCancel(Func<CancellationToken, Task> executeAsync)
        {
            var wrapped = Wrap(executeAsync);
            return async () =>
            {
                Cancel();
                await wrapped().ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Wraps a delegate so that it registers with this cancel command. The delegate is passed the <see cref="CancellationToken"/> of this cancel command. Any <see cref="OperationCanceledException"/> exceptions raised by the delegate are silently ignored.
        /// </summary>
        /// <param name="executeAsync">The cancelable delegate.</param>
        public Func<object?, Task> Wrap(Func<object?, CancellationToken, Task> executeAsync)
        {
            return async parameter =>
            {
                using (StartOperation())
                {
                    try
                    {
                        await executeAsync(parameter, _context?.Token ?? CancellationToken.None);
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
        public Func<Task> Wrap(Func<CancellationToken, Task> executeAsync)
        {
            var wrapped = Wrap((_, token) => executeAsync(token));
            return () => wrapped(null);
        }

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
        private sealed class RefCountedCancellationTokenSource
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
        {
            /// <summary>
            /// The parent <see cref="CancelCommand"/>.
            /// </summary>
            private readonly CancelCommand _parent;

            /// <summary>
            /// The cancellation token source.
            /// </summary>
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            /// <summary>
            /// The number of current operations. If this is <c>0</c>, then the cts is canceled.
            /// </summary>
            private int _count;

            public RefCountedCancellationTokenSource(CancelCommand parent)
            {
                _parent = parent;
            }

            public CancellationToken Token => _cts.Token;

            /// <summary>
            /// Decrements the count, and cancels the command if the new count is <c>0</c>.
            /// </summary>
            private void Signal()
            {
                if (--_count != 0)
                    return;
                Cancel();
            }

            /// <summary>
            /// Associates an operation with the cts. The operation is disassociated when disposed.
            /// </summary>
            public IDisposable StartOperation()
            {
                ++_count;
                return new AnonymousDisposable(Signal);
            }

            /// <summary>
            /// Cancels the command immediately.
            /// </summary>
            public void Cancel()
            {
                _cts.Cancel();
                _parent.Notify(this);
            }
        }
    }
}