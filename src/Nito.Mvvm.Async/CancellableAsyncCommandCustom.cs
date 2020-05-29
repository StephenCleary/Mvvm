namespace Nito.Mvvm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// An asynchronous command with ability to cancel itself where the user determines when it can execute.
    /// </summary>
    public class CancellableAsyncCommandCustom : CustomAsyncCommand, ICancellableAsyncCommand
    {
        /// <summary>
        /// Command to cancel corresponding <see cref="CancellableAsyncCommandCustom"/>
        /// </summary>
        public CancelCommand CancelCommand { get; } = new CancelCommand();

        /// <summary>
        /// Method for cancelling corresponding <see cref="CancellableAsyncCommandCustom"/>
        /// </summary>
        public void Cancel() => CancelCommand.Cancel();

        /// <summary>
        /// Creates a new cancellable asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CancellableAsyncCommandCustom(
            Func<object, CancellationToken, Task> executeAsync,
            Func<object, bool> canExecute,
            Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(_ => TaskExt.CompletedTask, canExecute, canExecuteChangedFactory) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new cancellable asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public CancellableAsyncCommandCustom(
            Func<object, CancellationToken, Task> executeAsync,
            Func<object, bool> canExecute)
            : base(_ => TaskExt.CompletedTask, canExecute) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new cancellable asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CancellableAsyncCommandCustom(
            Func<CancellationToken, Task> executeAsync,
            Func<bool> canExecute,
            Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(() => TaskExt.CompletedTask, canExecute, canExecuteChangedFactory) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));

        /// <summary>
        /// Creates a new cancellable asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public CancellableAsyncCommandCustom(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute)
            : base(() => TaskExt.CompletedTask, canExecute) =>
            SetExecutingFunc(CancelCommand.Wrap(executeAsync));
    }
}
