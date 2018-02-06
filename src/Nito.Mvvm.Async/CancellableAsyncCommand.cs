namespace Nito.Mvvm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// A basic asynchronous command with ability to cancel itself, which (by default) is disabled while the command is executing.
    /// </summary>
    public class CancellableAsyncCommand : AsyncCommand, ICancellableAsyncCommand
    {
        /// <summary>
        /// Command to cancel corresponding <see cref="CancellableAsyncCommand"/>
        /// </summary>
        public CancelCommand CancelCommand { get; } = new CancelCommand();

        /// <summary>
        /// Method for cancelling corresponding <see cref="CancellableAsyncCommand"/>
        /// </summary>
        public void Cancel() => CancelCommand.Cancel();

        /// <summary>
        /// Creates a new asynchronous cancellable command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CancellableAsyncCommand(
            Func<object, CancellationToken, Task> executeAsync,
            Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(_ => TaskExt.CompletedTask, canExecuteChangedFactory) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new asynchronous cancellable command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        public CancellableAsyncCommand(Func<object, CancellationToken, Task> executeAsync)
            : base(_ => TaskExt.CompletedTask) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new asynchronous cancellable command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CancellableAsyncCommand(
            Func<CancellationToken, Task> executeAsync,
            Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(() => TaskExt.CompletedTask, canExecuteChangedFactory) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new asynchronous cancellable command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        public CancellableAsyncCommand(Func<CancellationToken, Task> executeAsync)
            : base(() => TaskExt.CompletedTask) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));
    }
}
