using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// A trigger property: a source property that invalidates its targets when set.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    [DebuggerTypeProxy(typeof(TriggerProperty<>.DebugView))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class TriggerProperty<T> : ISourceProperty
    {
        private readonly SourceProperty _sourceProperty;
        private readonly IEqualityComparer<T> _comparer;
        private T _value;
        private Delegate? _collectionChangedHandler;

        /// <summary>
        /// Creates the trigger property.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="initialValue">The optional initial value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed.</param>
        public TriggerProperty(Action<PropertyChangedEventArgs> onPropertyChanged, T initialValue = default, IEqualityComparer<T>? comparer = null)
        {
            _sourceProperty = new SourceProperty(onPropertyChanged);
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _value = initialValue;
            Attach(_value);
        }

        /// <summary>
        /// Gets the value of the property. If dependency tracking is active, then this property is registered as a source for the target property being tracked.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public T GetValue([CallerMemberName] string propertyName = null!)
        {
            _sourceProperty.SetPropertyName(propertyName);
            DependencyTracker.Instance.Register(this);
            return _value;
        }

        /// <summary>
        /// Sets the value of the property. The internal value is always set to the new value. If the old value and new value are different (as defined by the comparer passed to the constructor), then this property and the transitive closure of all its target properties are invalidated. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        public void SetValue(T value, [CallerMemberName] string propertyName = null!)
        {
            _sourceProperty.SetPropertyName(propertyName);
            var equal = _comparer.Equals(_value, value);
            Detach(_value);
            _value = value;
            Attach(_value);
            if (!equal)
                _sourceProperty.Invalidate();
        }

        private void Attach(T value)
        {
            _collectionChangedHandler = ReflectionHelper.For<T>.AddEventHandler(this, value);
        }

        private void Detach(T value)
        {
            ReflectionHelper.For<T>.RemoveEventHandler(value, _collectionChangedHandler);
        }

        void ISourceProperty.AddTarget(ITargetProperty targetProperty)
        {
            _sourceProperty.AddTarget(targetProperty);
        }

        void ISourceProperty.RemoveTarget(ITargetProperty targetProperty)
        {
            _sourceProperty.RemoveTarget(targetProperty);
        }

        void IProperty.Invalidate()
        {
            _sourceProperty.Invalidate();
        }

        void IProperty.InvalidateTargets()
        {
            _sourceProperty.InvalidateTargets();
        }

        [DebuggerNonUserCode]
        private string DebuggerDisplay
        {
            get
            {
                var view = new DebugView(this);
                var name = view.Name ?? "<null>";
                var list = _value as System.Collections.IList;
                if (list == null)
                    return name + ": " + view.Value;
                return name + ": Count = " + list.Count;
            }
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly TriggerProperty<T> _property;
            private readonly SourceProperty.DebugView _base;

            public DebugView(TriggerProperty<T> property)
            {
                _property = property;
                _base = new SourceProperty.DebugView(property._sourceProperty);
            }

            public string? Name { get { return _base.Name; } }

            public T Value { get { return _property._value; } }

            public HashSet<ITargetProperty> Targets { get { return _base.Targets; } }

            public bool ListeningToCollectionChanged { get { return _property._collectionChangedHandler != null; } }
        }
    }
}
