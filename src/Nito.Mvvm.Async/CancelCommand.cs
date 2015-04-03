using System;
using System.Threading;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// A command that cancels a <see cref="CancellationToken"/> when it is executed.
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
        private CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Creates a new cancel command with a new cancellation token. This command is initially enabled, with an uncanceled cancellation token.
        /// </summary>
        public CancelCommand()
        {
            _canExecuteChanged = new WeakCanExecuteChanged(this);
        }

        /// <summary>
        /// Gets a cancellation token that will be canceled when this command is executed.
        /// </summary>
        public CancellationToken CancellationToken { get { return _cts.Token; } }

        /// <summary>
        /// Gets a value indicating whether the cancel command has been executed. Call <see cref="Reset"/> to reset the command to an uncancelled state.
        /// </summary>
        public bool IsCancellationRequested { get { return _cts.IsCancellationRequested; } }

        /// <summary>
        /// Executes the cancel command.
        /// </summary>
        public void Cancel()
        {
            if (_cts.IsCancellationRequested)
                return;
            _cts.Cancel();
            _canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Resets this cancel command to an uncancelled state. If the cancel command hasn't executed yet, then this method does nothing.
        /// </summary>
        public void Reset()
        {
            if (!_cts.IsCancellationRequested)
                return;
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
    }
}