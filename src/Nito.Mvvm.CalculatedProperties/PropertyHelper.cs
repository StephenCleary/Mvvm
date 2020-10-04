using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// Manages a collection of properties.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class PropertyHelper
    {
        private readonly Action<PropertyChangedEventArgs> _onPropertyChanged;
        private readonly Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();

        /// <summary>
        /// Creates a property collection with the specified method to raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="onPropertyChanged">A method that raises <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        public PropertyHelper(Action<PropertyChangedEventArgs> onPropertyChanged)
        {
            _onPropertyChanged = onPropertyChanged;
        }

        /// <summary>
        /// Retrieves the specified trigger property if it exists; otherwise, creates the named trigger property and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="calculateInitialValue">The optional delegate that returns the initial value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed.</param>
        /// <param name="propertyName">The name of the property to retrieve or create.</param>
        public TriggerProperty<T> GetOrAddTriggerProperty<T>(Func<T>? calculateInitialValue = null, IEqualityComparer<T>? comparer = null, [CallerMemberName] string propertyName = null!)
        {
            IProperty result;
            if (!_properties.TryGetValue(propertyName, out result))
            {
                result = new TriggerProperty<T>(_onPropertyChanged, calculateInitialValue == null ? default! : calculateInitialValue(), comparer);
                _properties.Add(propertyName, result);
            }

            return (TriggerProperty<T>) result;
        }

        /// <summary>
        /// Retrieves the specified calculated property if it exists; otherwise, creates the calculated property and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="calculateValue">The delegate used to calculate the property value.</param>
        /// <param name="propertyName">The name of the property to retrieve or create.</param>
        public CalculatedProperty<T> GetOrAddCalculatedProperty<T>(Func<T> calculateValue, [CallerMemberName] string propertyName = null!)
        {
            IProperty result;
            if (!_properties.TryGetValue(propertyName, out result))
            {
                result = new CalculatedProperty<T>(_onPropertyChanged, calculateValue);
                _properties.Add(propertyName, result);
            }

            return (CalculatedProperty<T>) result;
        }

        /// <summary>
        /// Retrieves the specified trigger property if it exists; otherwise, returns <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        public TriggerProperty<T> GetTriggerProperty<T>(string propertyName)
        {
            IProperty result;
            _properties.TryGetValue(propertyName, out result);
            return (TriggerProperty<T>) result;
        }

        /// <summary>
        /// Retrieves the specified calculated property if it exists; otherwise, returns <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        public CalculatedProperty<T> GetCalculatedProperty<T>(string propertyName)
        {
            IProperty result;
            _properties.TryGetValue(propertyName, out result);
            return (CalculatedProperty<T>) result;
        }

        /// <summary>
        /// Implements the getter for a trigger property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="calculateInitialValue">The delegate that returns the initial value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed. If this is specified, then the same comparer should be passed to the setter implementation.</param>
        /// <param name="propertyName">The name of the property.</param>
        public T Get<T>(Func<T> calculateInitialValue, IEqualityComparer<T>? comparer = null, [CallerMemberName] string propertyName = null!)
        {
            return GetOrAddTriggerProperty(calculateInitialValue, comparer, propertyName).GetValue(propertyName);
        }

        /// <summary>
        /// Implements the getter for a trigger property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed. If this is specified, then the same comparer should be passed to the setter implementation.</param>
        /// <param name="propertyName">The name of the property.</param>
        public T Get<T>(T initialValue, IEqualityComparer<T>? comparer = null, [CallerMemberName] string propertyName = null!)
        {
            return Get(() => initialValue, comparer, propertyName);
        }

        /// <summary>
        /// Implements the setter for a trigger property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="value">The new value of the property.</param>
        /// <param name="comparer">The optional comparer used to determine when the value of the property has changed. If this is specified, then the same comparer should be passed to the getter implementation.</param>
        /// <param name="propertyName">The name of the property.</param>
        public void Set<T>(T value, IEqualityComparer<T>? comparer = null, [CallerMemberName] string propertyName = null!)
        {
            GetOrAddTriggerProperty(() => value, comparer, propertyName).SetValue(value, propertyName);
        }

        /// <summary>
        /// Implements the getter for a calculated property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="calculateValue">The delegate used to calculate the property value.</param>
        /// <param name="propertyName">The name of the property.</param>
        public T Calculated<T>(Func<T> calculateValue, [CallerMemberName] string propertyName = null!)
        {
            return GetOrAddCalculatedProperty(calculateValue, propertyName).GetValue(propertyName);
        }

        [DebuggerNonUserCode]
        private string DebuggerDisplay
        {
            get { return "Count = " + _properties.Count; }
        }

        [DebuggerNonUserCode]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
        private sealed class DebugView
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
        {
            private readonly PropertyHelper _properties;

            public DebugView(PropertyHelper properties)
            {
                _properties = properties;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object Properties
            {
                get { return _properties._properties.Values.ToArray(); }
            }
        }
    }
}
