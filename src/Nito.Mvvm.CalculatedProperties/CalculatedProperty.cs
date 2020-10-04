using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// A calculated property: a property whose value is determined by a delegate. Calculated properties are target properties and may also be source properties.
    /// </summary>
    /// <typeparam name="T">The type of the property value returned by the delegate.</typeparam>
    [DebuggerTypeProxy(typeof(CalculatedProperty<>.DebugView))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class CalculatedProperty<T> : ISourceProperty, ITargetProperty
    {
        private readonly SourceProperty _sourceProperty;
        private readonly Func<T> _calculateValue;
        private readonly HashSet<ISourceProperty> _sources;
        private T _value = default!;
        private bool _valueIsValid;
        private Delegate? _collectionChangedHandler;

        /// <summary>
        /// Creates the calculated property.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="calculateValue">The delegate used to calculate the property value.</param>
        public CalculatedProperty(Action<PropertyChangedEventArgs> onPropertyChanged, Func<T> calculateValue)
        {
            _sourceProperty = new SourceProperty(onPropertyChanged);
            _calculateValue = calculateValue;
            _sources = new HashSet<ISourceProperty>();
        }

        /// <summary>
        /// Gets the value of the property. If the value has already been calculated and is valid, then the cached value is returned. Otherwise, a valid value is calculated (using dependency tracking) and returned.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public T GetValue([CallerMemberName] string propertyName = null!)
        {
            _sourceProperty.SetPropertyName(propertyName);
            DependencyTracker.Instance.Register(this);
            if (_valueIsValid)
                return _value;
            using (DependencyTracker.Instance.StartDependencyTracking(this))
            {
                _value = _calculateValue();
                _valueIsValid = true;
                Attach(_value);
                return _value;
            }
        }

        /// <summary>
        /// Queues <see cref="INotifyPropertyChanged.PropertyChanged"/> and invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        public void Invalidate()
        {
            _valueIsValid = false;
            Detach(_value);
            _value = default!;
            _sourceProperty.Invalidate();
        }

        ISet<ISourceProperty> ITargetProperty.Sources
        {
            get { return _sources; }
        }

        void ITargetProperty.UpdateSources(ISet<ISourceProperty> sourcesToRemove, ISet<ISourceProperty>? sourcesToAdd)
        {
            _sources.ExceptWith(sourcesToRemove);
            if (sourcesToAdd != null)
                _sources.UnionWith(sourcesToAdd);
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
                if (!_valueIsValid)
                    return name + ": <invalid>";
                var list = _value as System.Collections.IList;
                if (list == null)
                    return name + ": " + view.Value;
                return name + ": Count = " + list.Count;
            }
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly CalculatedProperty<T> _property;
            private readonly SourceProperty.DebugView _base;

            public DebugView(CalculatedProperty<T> property)
            {
                _property = property;
                _base = new SourceProperty.DebugView(property._sourceProperty);
            }

            public string? Name { get { return _base.Name; } }

            public bool IsValid { get { return _property._valueIsValid; } }

            public T Value { get { return _property._value; } }

            public HashSet<ISourceProperty> Sources { get { return _property._sources; } } 

            public HashSet<ITargetProperty> Targets { get { return _base.Targets; } }

            public bool ListeningToCollectionChanged { get { return _property._collectionChangedHandler != null; } }
        }
    }
}
