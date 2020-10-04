using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// A base type for source properties.
    /// </summary>
    internal sealed class SourceProperty : ISourceProperty, IRaisePropertyChanged
    {
        private readonly int _threadId;
        private readonly Action<PropertyChangedEventArgs> _onPropertyChanged;
        private readonly HashSet<ITargetProperty> _targets;
        private string? _propertyName;

        /// <summary>
        /// Initializes the base type with the specified method to raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        public SourceProperty(Action<PropertyChangedEventArgs> onPropertyChanged)
        {
            _threadId = Environment.CurrentManagedThreadId;
            _onPropertyChanged = onPropertyChanged;
            _targets = new HashSet<ITargetProperty>();
        }

        /// <summary>
        /// Sets the property name to the specified string.
        /// </summary>
        /// <param name="propertyName">The name of this property.</param>
        public void SetPropertyName(string propertyName)
        {
            if (_threadId != Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("Cross-thread access detected.");

            if (propertyName == null)
            {
                Debug.WriteLine("CalculatedProperties: Property name set to null!");
                return;
            }

            if (_propertyName != null)
            {
                if (_propertyName != propertyName)
                    Debug.WriteLine("CalculatedProperties: Property name changed from " + _propertyName + " to " + propertyName);
                return;
            }

            _propertyName = propertyName;
        }

        /// <summary>
        /// Queues <see cref="INotifyPropertyChanged.PropertyChanged"/> and invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        public void Invalidate()
        {
            // Ensure notifications are deferred.
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                // Queue OnNotifyPropertyChanged.
                if (_propertyName == null)
                    Debug.WriteLine("CalculatedProperties: Cannot invoke OnPropertyChanged for an unnamed property.");
                else
                    PropertyChangedNotificationManager.Instance.Register(this, _propertyName);

                // Invalidate all targets.
                foreach (var target in _targets)
                    target.Invalidate();
            }
        }

        /// <summary>
        /// Invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning. <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised for this property.
        /// </summary>
        public void InvalidateTargets()
        {
            // Ensure notifications are deferred.
            using (PropertyChangedNotificationManager.Instance.DeferNotifications())
            {
                // Invalidate all targets.
                foreach (var target in _targets)
                    target.Invalidate();
            }
        }

        public void AddTarget(ITargetProperty targetProperty)
        {
            _targets.Add(targetProperty);
        }

        public void RemoveTarget(ITargetProperty targetProperty)
        {
            _targets.Remove(targetProperty);
        }

        void IRaisePropertyChanged.RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            _onPropertyChanged(e);
        }

        [DebuggerNonUserCode]
        public sealed class DebugView
        {
            private readonly SourceProperty _property;

            /// <summary>
            /// Creates a debug view for the specified property.
            /// </summary>
            /// <param name="property">The property to examine.</param>
            public DebugView(SourceProperty property)
            {
                _property = property;
            }

            /// <summary>
            /// Gets the property name.
            /// </summary>
            public string? Name { get { return _property._propertyName; } }

            /// <summary>
            /// Gets the target properties.
            /// </summary>
            public HashSet<ITargetProperty> Targets { get { return _property._targets; } }
        }
    }
}
