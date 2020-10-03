using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// An implementation of <c>ICommand.CanExecuteChanged</c> with <c>ICommand.CanExecute</c>.
    /// </summary>
    public interface ICanExecute
    {
        /// <summary>
        /// Occurs when the return value of <see cref="CanExecute"/> may have changed. This is a weak event.
        /// </summary>
        event EventHandler CanExecuteChanged;

        /// <summary>
        /// Whether the command can execute.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        bool CanExecute(object parameter);
    }
}