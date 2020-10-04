using System;
using System.Collections.Generic;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// Tracks dependencies (sources) for a given target property.
    /// </summary>
    internal sealed class DependencyTracker
    {
        [ThreadStatic]
        private static DependencyTracker? SingletonInstance;
        private readonly Stack<StackFrame?> _stack;

        private DependencyTracker()
        {
            _stack = new Stack<StackFrame?>();
            _stack.Push(null);
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DependencyTracker Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new DependencyTracker();
                return SingletonInstance;
            }
        }

        /// <summary>
        /// Starts tracking dependencies for the specified target property. Stops tracking dependencies when the returned disposable is disposed. Dependencies are tracked using a stack, so this is safe to call for a target property that has other target properties as its source.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        public IDisposable StartDependencyTracking(ITargetProperty targetProperty)
        {
            _stack.Push(new StackFrame(targetProperty));
            return StopDependencyTrackingWhenDisposed.Instance;
        }

        /// <summary>
        /// Registers the specified property as a source for the target property currently being tracked. If no target property is currently being tracked, then this method does nothing.
        /// </summary>
        /// <param name="sourceProperty">The source property.</param>
        public void Register(ISourceProperty sourceProperty)
        {
            var currentFrame = _stack.Peek();
            if (currentFrame == null)
                return;
            currentFrame.Register(sourceProperty);
        }

        private void StopDependencyTracking()
        {
            _stack.Pop()?.UpdatePropertyDependencies();
        }

        private sealed class StackFrame
        {
            private readonly ITargetProperty _targetProperty;
            private readonly HashSet<ISourceProperty> _sourcesToRemove;
            private HashSet<ISourceProperty>? _sourcesToAdd;

            public StackFrame(ITargetProperty targetProperty)
            {
                _targetProperty = targetProperty;
                _sourcesToRemove = new HashSet<ISourceProperty>(_targetProperty.Sources);
            }

            public void Register(ISourceProperty source)
            {
                if (_sourcesToRemove.Remove(source))
                    return;
                if (_sourcesToAdd == null)
                    _sourcesToAdd = new HashSet<ISourceProperty>();
                _sourcesToAdd.Add(source);
                source.AddTarget(_targetProperty);
            }

            public void UpdatePropertyDependencies()
            {
                _targetProperty.UpdateSources(_sourcesToRemove, _sourcesToAdd);
                foreach (var source in _sourcesToRemove)
                    source.RemoveTarget(_targetProperty);
            }
        }

        private sealed class StopDependencyTrackingWhenDisposed : IDisposable
        {
            public static StopDependencyTrackingWhenDisposed Instance = new StopDependencyTrackingWhenDisposed();

            public void Dispose()
            {
                DependencyTracker.Instance.StopDependencyTracking();
            }
        }
    }
}
