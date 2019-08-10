using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// A strongly typed parameterized asynchronous command, which forwards it's implementation to <see cref="AsyncCommand"/>.
    /// </summary>
    public sealed class AsyncCommand<T> : AsyncCommandBase, IAsyncCommand<T>, INotifyPropertyChanged
    {
        private readonly AsyncCommand _asyncCommand;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand{T}.ExecuteAsync(T)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public AsyncCommand(Func<T, Task> executeAsync, Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(canExecuteChangedFactory)
        {
            _asyncCommand = new AsyncCommand(async parameter => await executeAsync((T)parameter), canExecuteChangedFactory);
            _asyncCommand.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_asyncCommand.IsExecuting):
                        PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(nameof(IsExecuting)));
                        break;
                    case nameof(_asyncCommand.Execution):
                        PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(nameof(Execution)));
                        break;
                }
            };
            ((ICommand)_asyncCommand).CanExecuteChanged += (object sender, EventArgs e) => OnCanExecuteChanged();
        }

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand{T}.ExecuteAsync(T)"/>.</param>
        public AsyncCommand(Func<T, Task> executeAsync)
            : this(executeAsync, CanExecuteChangedFactories.DefaultCanExecuteChangedFactory)
        {
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Represents the most recent execution of the asynchronous command. Returns <c>null</c> until the first execution of this command.
        /// </summary>
        public NotifyTask Execution => _asyncCommand.Execution;

        /// <summary>
        /// Whether the asynchronous command is currently executing.
        /// </summary>
        public bool IsExecuting => _asyncCommand.IsExecuting;


        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. Returns <c>false</c> whenever the async command is in progress.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected override bool CanExecute(object parameter) => ((ICommand)_asyncCommand).CanExecute(parameter);

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override Task ExecuteAsync(object parameter)
        {
            return ExecuteAsync((T)parameter);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public Task ExecuteAsync(T parameter)
        {
            return _asyncCommand.ExecuteAsync(parameter);
        }
    }
}