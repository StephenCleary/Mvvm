using Nito.Disposables;
using System;
using System.ComponentModel;

namespace Nito.Mvvm
{
    /// <summary>
    /// A subscription to a property changed event for a particular property.
    /// </summary>
    public sealed class PropertyChangedSubscription: SingleDisposable<INotifyPropertyChanged>
    {
        /// <summary>
        /// The name of the property being observed. May be <c>null</c>.
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// The callback to invoke when the property changed. Never <c>null</c>.
        /// </summary>
        private readonly PropertyChangedEventHandler _handler;

        /// <summary>
        /// The actual subscription to <see cref="INotifyPropertyChanged.PropertyChanged"/>. Never <c>null</c>.
        /// </summary>
        private readonly PropertyChangedEventHandler _subscription;

        /// <summary>
        /// This object is thread-affine.
        /// </summary>
        private ThreadAffinity _threadAffinity;

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for a particular property.
        /// </summary>
        /// <param name="source">The object whose property is observed. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property to observe. May be <c>null</c> to indicate that all properties should be observed.</param>
        /// <param name="handler">The callback that is called when the property has changed. May not be <c>null</c>.</param>
        public PropertyChangedSubscription(INotifyPropertyChanged source, string propertyName, PropertyChangedEventHandler handler)
            : base(source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _threadAffinity = ThreadAffinity.BindToCurrentThread();
            _propertyName = propertyName;
            _handler = handler;

            _subscription = SourceOnPropertyChanged;
            source.PropertyChanged += _subscription;
        }

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for a particular property.
        /// </summary>
        /// <param name="source">The object whose property is observed. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property to observe. May be <c>null</c> to indicate that all properties should be observed.</param>
        /// <param name="handler">The callback that is called when the property has changed. May not be <c>null</c>.</param>
        public PropertyChangedSubscription(INotifyPropertyChanged source, string propertyName, Action handler)
            : this(source, propertyName, (_, __) => handler())
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
        }

        private void SourceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            _threadAffinity.VerifyCurrentThread();
            if (propertyChangedEventArgs.PropertyName == _propertyName || _propertyName == null)
                _handler(sender, propertyChangedEventArgs);
        }

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for a particular property.
        /// </summary>
        /// <param name="source">The object whose property is observed. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property to observe. May be <c>null</c> to indicate that all properties should be observed.</param>
        /// <param name="handler">The callback that is called when the property has changed. May not be <c>null</c>.</param>
        /// <returns>The new subscription.</returns>
        public static IDisposable Create(INotifyPropertyChanged source, string propertyName, PropertyChangedEventHandler handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return new PropertyChangedSubscription(source, propertyName, handler);
        }

        /// <summary>
        /// Subscribes to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for a particular property.
        /// </summary>
        /// <param name="source">The object whose property is observed. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property to observe. May be <c>null</c> to indicate that all properties should be observed.</param>
        /// <param name="handler">The callback that is called when the property has changed. May not be <c>null</c>.</param>
        /// <returns>The new subscription.</returns>
        public static IDisposable Create(INotifyPropertyChanged source, string propertyName, Action handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return new PropertyChangedSubscription(source, propertyName, handler);
        }

        /// <summary>
        /// Unsubscribes from the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        protected override void Dispose(INotifyPropertyChanged context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _threadAffinity.VerifyCurrentThread();
            context.PropertyChanged -= _subscription;
        }
    }
}
