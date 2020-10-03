using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// Class that implements <c>ICommand.CanExecuteChanged</c> as a strong event.
    /// </summary>
    public sealed class StrongCanExecuteChanged : ICanExecuteChanged
    {
        /// <summary>
        /// The sender of the <c>ICommand.CanExecuteChanged</c> event.
        /// </summary>
        private readonly object _sender;

        /// <summary>
        /// The collection of delegates for <see cref="CanExecuteChanged"/>.
        /// </summary>
        private event EventHandler? _canExecuteChanged;

        /// <summary>
        /// This object is thread-affine.
        /// </summary>
        private ThreadAffinity _threadAffinity;

        /// <summary>
        /// Creates a new strong-event implementation of <c>ICommand.CanExecuteChanged</c>.
        /// </summary>
        /// <param name="sender">The sender of the <c>ICommand.CanExecuteChanged</c> event.</param>
        public StrongCanExecuteChanged(object sender)
        {
            _threadAffinity = ThreadAffinity.BindToCurrentThread();
            _sender = sender;
        }

        /// <summary>
        /// Provides notification that the result of <c>ICommand.CanExecute</c> may be different.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                _threadAffinity.VerifyCurrentThread();
                _canExecuteChanged += value;
            }
            remove
            {
                _threadAffinity.VerifyCurrentThread();
                _canExecuteChanged -= value;
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            _threadAffinity.VerifyCurrentThread();
            _canExecuteChanged?.Invoke(_sender, EventArgs.Empty);
        }
    }
}