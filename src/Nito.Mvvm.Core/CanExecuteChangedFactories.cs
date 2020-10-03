using System;
using System.Collections.Generic;

namespace Nito.Mvvm
{
    /// <summary>
    /// Global factory methods for creating <see cref="ICanExecuteChanged"/> implementations.
    /// </summary>
    public static class CanExecuteChangedFactories
    {
        /// <summary>
        /// Factory method for creating weak implementations of <see cref="ICanExecuteChanged"/>.
        /// </summary>
        public static readonly Func<object, ICanExecuteChanged> WeakCanExecuteChangedFactory = sender => new WeakCanExecuteChanged(sender);

        /// <summary>
        /// Factory method for creating strong implementations of <see cref="ICanExecuteChanged"/>.
        /// </summary>
        public static readonly Func<object, ICanExecuteChanged> StrongCanExecuteChangedFactory = sender => new StrongCanExecuteChanged(sender);

        /// <summary>
        /// Default factory method for creating <see cref="ICanExecuteChanged"/> implementations.
        /// </summary>
        public static Func<object, ICanExecuteChanged> DefaultCanExecuteChangedFactory { get; set; } = StrongCanExecuteChangedFactory;
    }
}
