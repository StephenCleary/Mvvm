using System;
using System.ComponentModel;

namespace Nito.Mvvm
{
    /// <summary>
    /// An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. This interface is typically implemented explicitly.
    /// </summary>
    public interface IRaisePropertyChanged
    {
        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with the specified arguments.
        /// </summary>
        /// <param name="args">The event arguments to pass to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
#pragma warning disable CA1030 // Use events where appropriate
        void RaisePropertyChanged(PropertyChangedEventArgs args);
#pragma warning restore CA1030 // Use events where appropriate
    }
}