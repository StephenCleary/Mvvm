using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// A basic asynchronous command, which (by default) is disabled while the command is executing.
    /// </summary>
    public sealed class AsyncCommand : AsyncCommandBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private readonly Func<object, Task> _executeAsync;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        public AsyncCommand(Func<object, Task> executeAsync)
        {
            _executeAsync = executeAsync;
        }

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        public AsyncCommand(Func<Task> executeAsync)
            : this(_ => executeAsync())
        {
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
        /// Executes the asynchronous command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter)
        {
            var tcs = new TaskCompletionSource<object>();
            Execution = NotifyTask.Create(DoExecuteAsync(tcs.Task, _executeAsync, parameter));
            OnCanExecuteChanged();
            var propertyChanged = PropertyChanged;
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("Execution"));
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            tcs.SetResult(null);
            await Execution.TaskCompleted;
            OnCanExecuteChanged();
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            await Execution.Task;
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. Returns <c>false</c> whenever the async command is in progress.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected override bool CanExecute(object parameter) => !IsExecuting;

        private static async Task DoExecuteAsync(Task precondition, Func<object, Task> executeAsync, object parameter)
        {
            await precondition;
            await executeAsync(parameter);
        }
    }
}