using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// Class that implements <c>ICommand.CanExecuteChanged</c> as a weak event.
    /// </summary>
    public sealed class WeakCanExecuteChanged : ICanExecuteChanged
    {
        /// <summary>
        /// The sender of the <c>ICommand.CanExecuteChanged</c> event.
        /// </summary>
        private readonly object _sender;

        /// <summary>
        /// The weak collection of delegates for <see cref="CanExecuteChanged"/>.
        /// </summary>
        private readonly WeakCollection<EventHandler> _canExecuteChanged = new WeakCollection<EventHandler>();

        /// <summary>
        /// This object is thread-affine.
        /// </summary>
        private ThreadAffinity _threadAffinity;

        /// <summary>
        /// Creates a new weak-event implementation of <c>ICommand.CanExecuteChanged</c>.
        /// </summary>
        /// <param name="sender">The sender of the <c>ICommand.CanExecuteChanged</c> event.</param>
        public WeakCanExecuteChanged(object sender)
        {
            _threadAffinity = ThreadAffinity.BindToCurrentThread();
            _sender = sender;
        }

        /// <summary>
        /// This is a weak event. Provides notification that the result of <c>ICommand.CanExecute</c> may be different.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                _threadAffinity.VerifyCurrentThread();
                _canExecuteChanged.Add(value);
            }
            remove
            {
                _threadAffinity.VerifyCurrentThread();
                _canExecuteChanged.Remove(value);
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event for any listeners still alive, and removes any references to garbage collected listeners.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            _threadAffinity.VerifyCurrentThread();
            foreach (var canExecuteChanged in _canExecuteChanged.GetLiveItems())
                canExecuteChanged(_sender, EventArgs.Empty);
        }
    }
}