using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// An asynchronous command where the user determines when it can execute.
    /// </summary>
    public sealed class CustomAsyncCommand : AsyncCommandBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private readonly Func<object, Task> _executeAsync;

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>.
        /// </summary>
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CustomAsyncCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute, Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : base(canExecuteChangedFactory)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public CustomAsyncCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute)
            : this(executeAsync, canExecute, CanExecuteChangedFactories.DefaultCanExecuteChangedFactory)
        {
        }

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        /// <param name="canExecuteChangedFactory">The factory for the implementation of <see cref="ICommand.CanExecuteChanged"/>.</param>
        public CustomAsyncCommand(Func<Task> executeAsync, Func<bool> canExecute, Func<object, ICanExecuteChanged> canExecuteChangedFactory)
            : this(_ => executeAsync(), _ => canExecute(), canExecuteChangedFactory)
        {
        }

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="executeAsync">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        /// <param name="canExecute">The implementation of <see cref="ICommand.CanExecute(object)"/>.</param>
        public CustomAsyncCommand(Func<Task> executeAsync, Func<bool> canExecute)
            : this(_ => executeAsync(), _ => canExecute(), CanExecuteChangedFactories.DefaultCanExecuteChangedFactory)
        {
        }

        /// <summary>
        /// Represents the most recent execution of the asynchronous command. Returns <c>null</c> until the first execution of this command.
        /// </summary>
        public NotifyTask? Execution { get; private set; }

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
        /// Executes the asynchronous command. Any exceptions from the asynchronous delegate are captured and placed on <see cref="Execution"/>; they are not propagated to the UI loop.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter)
        {
            var tcs = new TaskCompletionSource<object>();
            Execution = NotifyTask.Create(DoExecuteAsync(tcs.Task, _executeAsync, parameter));
            var propertyChanged = PropertyChanged;
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(nameof(Execution)));
            propertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(nameof(IsExecuting)));
            tcs.SetResult(null!);
            await Execution.TaskCompleted;
            PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Instance.Get(nameof(IsExecuting)));
            await Execution.Task;
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. Invokes the <c>canExecute</c> delegate that was passed to the constructor.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected override bool CanExecute(object parameter) => _canExecute(parameter);

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        public new void OnCanExecuteChanged() => base.OnCanExecuteChanged();

        private static async Task DoExecuteAsync(Task precondition, Func<object, Task> executeAsync, object parameter)
        {
            await precondition;
            await executeAsync(parameter);
        }
    }
}