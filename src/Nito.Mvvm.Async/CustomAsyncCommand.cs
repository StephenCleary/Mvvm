using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nito.Mvvm
{
    /// <summary>
    /// An asynchronous command where the user determines when it can execute.
    /// </summary>
    public class CustomAsyncCommand : AsyncCommandBaseExtended
    {
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
            : base(executeAsync, canExecuteChangedFactory)
        {
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
        /// Notify about chnaging can execute state
        /// </summary>
        protected override void RaiseCanExecuteChanged()
        {
        }

        /// <summary>
        /// The implementation of <see cref="ICommand.CanExecute(object)"/>. Invokes the <c>canExecute</c> delegate that was passed to the constructor.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        protected override bool CanExecute(object parameter) => _canExecute(parameter);

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        public new void OnCanExecuteChanged() => base.OnCanExecuteChanged();
    }
}