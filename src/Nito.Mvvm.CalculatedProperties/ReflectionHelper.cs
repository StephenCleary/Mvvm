using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nito.Mvvm.CalculatedProperties;

/// <summary>
/// Provides methods (with caching) to assist with reflection.
/// </summary>
internal static class ReflectionHelper
{
    private static Type? _iNotifyCollectionChangedType;
    private static Type? _notifyCollectionChangedEventHandlerType;
    private static Type? _notifyCollectionChangedEventArgsType;
    private static EventInfo? _collectionChangedEvent;

    // This method does nothing; it only exists to make static field initialization deterministic.
    static ReflectionHelper()
    {
    }

    /// <summary>
    /// Provides methods (with caching) to assist with reflection over a specific type.
    /// </summary>
    /// <typeparam name="T">The type being reflected over.</typeparam>
    public static class For<T>
    {
        private static readonly bool ImplementsINotifyCollectionChanged;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static For()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            var interfaces = typeof(T).GetTypeInfo().ImplementedInterfaces;
            ImplementsINotifyCollectionChanged = DetectINotifyCollectionChanged(interfaces);
        }

        private static bool DetectINotifyCollectionChanged(IEnumerable<Type> interfaces)
        {
            if (_collectionChangedEvent != null)
                return interfaces.Contains(_iNotifyCollectionChangedType);
            _iNotifyCollectionChangedType = interfaces.FirstOrDefault(x => x.FullName == "System.Collections.Specialized.INotifyCollectionChanged");
            if (_iNotifyCollectionChangedType == null)
                return false;
            var assembly = _iNotifyCollectionChangedType.GetTypeInfo().Assembly;
            _notifyCollectionChangedEventHandlerType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventHandler");
            _notifyCollectionChangedEventArgsType = assembly.GetType("System.Collections.Specialized.NotifyCollectionChangedEventArgs");
            _collectionChangedEvent = _iNotifyCollectionChangedType.GetTypeInfo().GetDeclaredEvent("CollectionChanged");
            return true;
        }

        /// <summary>
        /// Adds a <c>INotifyCollectionChanged.CollectionChanged</c> event handler that calls <see cref="IProperty.InvalidateTargets"/> on the specified property. Returns the subscribed delegate, or <c>null</c> if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="property">The property whose targets should be invalidated. May not be <c>null</c>.</param>
        /// <param name="value">The value to observe. May be <c>null</c>.</param>
        public static Delegate? AddEventHandler(IProperty property, T value)
        {
            if (!ImplementsINotifyCollectionChanged || value == null)
                return null;

            Delegate result;

            // (object sender, NotifyCollectionChangedEventArgs e) => property.InvalidateTargets();
            var sender = Expression.Parameter(typeof(object), "sender");
            var args = Expression.Parameter(_notifyCollectionChangedEventArgsType, "e");
            var lambda = Expression.Lambda(_notifyCollectionChangedEventHandlerType,
                Expression.Call(Expression.Constant(property, typeof(IProperty)), "InvalidateTargets", null),
                sender, args);
            result = lambda.Compile();

            _collectionChangedEvent!.AddEventHandler(value, result);

            return result;
        }

        /// <summary>
        /// Removes a <c>INotifyCollectionChanged.CollectionChanged</c> event handler from the specified value. Does nothing if <typeparamref name="T"/> does not implement <c>INotifyCollectionChanged</c> or if <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">The value being observed. May be <c>null</c>.</param>
        /// <param name="handler">The delegate to be unsubscribed.</param>
        public static void RemoveEventHandler(T value, Delegate? handler)
        {
            if (!ImplementsINotifyCollectionChanged || value == null)
                return;
            _collectionChangedEvent!.RemoveEventHandler(value, handler);
        }
    }
}