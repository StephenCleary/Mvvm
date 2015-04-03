using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// An async version of <see cref="ICommand"/>.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }

    /// <summary>
    /// An async command that implements <see cref="ICommand"/>, forwarding <see cref="ICommand.Execute(object)"/> to <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
    /// </summary>
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        /// <summary>
        /// The local implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        private readonly WeakCanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public abstract Task ExecuteAsync(object parameter);

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected abstract bool CanExecute(object parameter);

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        protected void OnCanExecuteChanged()
        {
            _canExecuteChanged.OnCanExecuteChanged();
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { _canExecuteChanged.CanExecuteChanged += value; }
            remove { _canExecuteChanged.CanExecuteChanged -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        async void ICommand.Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }
    }

    /// <summary>
    /// A basic asynchronous command, which is disabled while the command is executing.
    /// </summary>
    public sealed class AsyncCommand : AsyncCommandBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.
        /// </summary>
        private readonly Func<object, Task> _func;

        /// <summary>
        /// Creates a new asynchronous command, with the specified asynchronous delegate as its implementation.
        /// </summary>
        /// <param name="func">The implementation of <see cref="IAsyncCommand.ExecuteAsync(object)"/>.</param>
        public AsyncCommand(Func<object, Task> func)
        {
            _func = func;
        }

        /// <summary>
        /// Represents the execution of the asynchronous command.
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
        /// Executes the asynchronous command. Any exceptions from the asynchronous delegate are captured and placed on <see cref="Execution"/>; they are not propagated to the UI loop.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public override async Task ExecuteAsync(object parameter)
        {
            Execution = NotifyTask.Create(_func(parameter));
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("Execution"));
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
            }
            await Execution.TaskCompleted;
            propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, PropertyChangedEventArgsCache.Instance.Get("IsExecuting"));
        }

        /// <summary>
        /// Raised when any properties on this instance have changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected override bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }
    }
}