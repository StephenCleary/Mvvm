using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// An implementation of <c>ICommand.CanExecuteChanged</c>.
    /// </summary>
    public interface ICanExecuteChanged
    {
        /// <summary>
        /// Provides notification that the result of <c>ICommand.CanExecute</c> may be different.
        /// </summary>
        event EventHandler CanExecuteChanged;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        void OnCanExecuteChanged();
    }
}