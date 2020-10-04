using System.ComponentModel;

namespace Nito.Mvvm.CalculatedProperties
{
    /// <summary>
    /// A property instance.
    /// </summary>
    internal interface IProperty
    {
        /// <summary>
        /// Queues <see cref="INotifyPropertyChanged.PropertyChanged"/> and invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates this property and the transitive closure of all its target properties. If notifications are not deferred, then this method will raise <see cref="INotifyPropertyChanged.PropertyChanged"/> for all affected properties before returning. <see cref="INotifyPropertyChanged.PropertyChanged"/> is not raised for this property.
        /// </summary>
        void InvalidateTargets();
    }
}
