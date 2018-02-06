namespace Nito.Mvvm
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// An async command that is derived from <see cref="AsyncCommandBase"/> and implements common logic for async command classes.
    /// </summary>
    public abstract class AsyncCommandBaseExtended : AsyncCommandBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private Func<object, Task> _executeAsync;

        /// <summary>
        /// Creates an instance with its own implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        protected AsyncCommandBaseExtended(Func<object, Task> executeAsync, Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(canExecuteChangedFactory)
        {
            _executeAsync = executeAsync;
        }

        /// <summary>
        /// Represents the most recent execution of the asynchronous command. Returns <c>null</c> until the first execution of this command.
        /// </summary>
        public NotifyTask Execution { get; private set; }

        /// <summary>
        /// Whether the asynchronous command is currently executing.
        /// </summary>
        public bool IsExecuting
        {
            get
            {
                if (Execution == null)
                    return false;
                return Execution.IsNotCompleted;
            }
        }

        /// <summary>
        /// Setter method for the function which is executing every time when command invoking.
        /// </summary>
        /// <param name="executeAsync">The function which will be executed next time command will be invoked.</param>
        public void SetExecutingFunc(Func<object, Task> executeAsync) => _executeAsync = executeAsync;

        /// <summary>
        /// Setter method for the function which is executing every time when command invoking.
        /// </summary>
        /// <param name="executeAsync">The function which will be executed next time command will be invoked.</param>
        public void SetExecutingFunc(Func<Task> executeAsync) => _executeAsync = _ => executeAsync();

        /// <summary>
        /// Executes the asynchronous command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter)
        {
            var tcs = new TaskCompletionSource<object>();
            Execution = NotifyTask.Create(DoExecuteAsync(tcs.Task, _executeAsync, parameter));
            RaiseCanExecuteChanged();
            OnPropertyChanged("Execution");
            OnPropertyChanged("IsExecuting");
            tcs.SetResult(null);
            await Execution.TaskCompleted;
            RaiseCanExecuteChanged();
            OnPropertyChanged("IsExecuting");
            await Execution.Task;
        }

        /// <summary>
        /// Notify about chnaging can execute state
        /// </summary>
        protected abstract void RaiseCanExecuteChanged();

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper method for raising <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propName"> property name </param>
        protected void OnPropertyChanged(string propName)
        {
            Volatile.Read(ref PropertyChanged)?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(propName));
        }

        private static async Task DoExecuteAsync(Task precondition, Func<object, Task> executeAsync, object parameter)
        {
            await precondition;
            await executeAsync(parameter);
        }
    }
}
