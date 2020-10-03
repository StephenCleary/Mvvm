using Nito.Disposables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nito.Mvvm
{
    /// <summary>
    /// Defers and consolidates <see cref="INotifyPropertyChanged.PropertyChanged"/> events. Events may be raised out of order when notifications are resumed. Instances of <see cref="IRaisePropertyChanged"/> are compared using reference equality during consolidation.
    /// </summary>
    [DebuggerTypeProxy(typeof(DebugView))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class PropertyChangedNotificationManager
    {
        [ThreadStatic]
        private static PropertyChangedNotificationManager? _singletonInstance;
        private readonly HashSet<PropertyChangedNotification> _propertiesRequiringNotification = new HashSet<PropertyChangedNotification>();
        private int _referenceCount;

        /// <summary>
        /// This object is thread-affine.
        /// </summary>
        private ThreadAffinity _threadAffinity;

        private PropertyChangedNotificationManager()
        {
            _threadAffinity = ThreadAffinity.BindToCurrentThread();
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PropertyChangedNotificationManager Instance
        {
            get
            {
                if (_singletonInstance == null)
                    _singletonInstance = new PropertyChangedNotificationManager();
                return _singletonInstance;
            }
        }

        /// <summary>
        /// Defers <see cref="INotifyPropertyChanged.PropertyChanged"/> events until the returned disposable is disposed. Deferrals are reference counted, so they are safe to nest.
        /// </summary>
        public IDisposable DeferNotifications()
        {
            _threadAffinity.VerifyCurrentThread();
            ++_referenceCount;
            return new ResumeOnDispose(this);
        }

        private void ResumeNotifications()
        {
            _threadAffinity.VerifyCurrentThread();
            --_referenceCount;
            if (_referenceCount != 0)
                return;
            var properties = new PropertyChangedNotification[_propertiesRequiringNotification.Count];
            _propertiesRequiringNotification.CopyTo(properties);
            _propertiesRequiringNotification.Clear();
            foreach (var property in properties)
                property.RaisePropertyChanged.RaisePropertyChanged(property.EventArgs);
        }

        /// <summary>
        /// Registers a <see cref="INotifyPropertyChanged.PropertyChanged"/> event. If events are not deferred, then the event is raised immediately.
        /// </summary>
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. May not be <c>null</c>.</param>
        /// <param name="args">The event arguments to pass to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, PropertyChangedEventArgs args)
        {
            _threadAffinity.VerifyCurrentThread();
            if (raisePropertyChanged == null)
                throw new ArgumentNullException(nameof(raisePropertyChanged));

            if (_referenceCount == 0)
                raisePropertyChanged.RaisePropertyChanged(args);
            else
                _propertiesRequiringNotification.Add(new PropertyChangedNotification(raisePropertyChanged, args));
        }

        /// <summary>
        /// Registers a <see cref="INotifyPropertyChanged.PropertyChanged"/> event. If events are not deferred, then the event is raised immediately.
        /// </summary>
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, string propertyName)
        {
            _threadAffinity.VerifyCurrentThread();
            Register(raisePropertyChanged, PropertyChangedEventArgsCache.Instance.Get(propertyName));
        }

        private sealed class ResumeOnDispose : SingleDisposable<PropertyChangedNotificationManager>
        {
            public ResumeOnDispose(PropertyChangedNotificationManager parent)
                : base(parent)
            {
            }

            protected override void Dispose(PropertyChangedNotificationManager context)
            {
                context.ResumeNotifications();
            }
        }

        private struct PropertyChangedNotification : IEquatable<PropertyChangedNotification>
        {
            public PropertyChangedNotification(IRaisePropertyChanged raisePropertyChanged, PropertyChangedEventArgs eventArgs)
            {
                RaisePropertyChanged = raisePropertyChanged;
                EventArgs = eventArgs;
            }

            public readonly IRaisePropertyChanged RaisePropertyChanged;
            public readonly PropertyChangedEventArgs EventArgs;

            public bool Equals(PropertyChangedNotification other)
            {
                return ReferenceEquals(RaisePropertyChanged, other.RaisePropertyChanged) && EventArgs.PropertyName == other.EventArgs.PropertyName;
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(RaisePropertyChanged) ^ EventArgs.PropertyName.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is PropertyChangedNotification notification && Equals(notification);
            }
        }

        [DebuggerNonUserCode]
        private string DebuggerDisplay
        {
            get
            {
                if (_referenceCount == 0)
                    return "Not deferred";
                return "Deferred; notification count: " + _propertiesRequiringNotification.Count + ", defer refcount: " + _referenceCount;
            }
        }

        [DebuggerNonUserCode]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
        private sealed class DebugView
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
        {
            private readonly PropertyChangedNotificationManager _value;

            public DebugView(PropertyChangedNotificationManager value)
            {
                _value = value;
            }

            public int ReferenceCount => _value._referenceCount;

            public HashSet<PropertyChangedNotification> DeferredNotifications => _value._propertiesRequiringNotification;
        }
    }
}