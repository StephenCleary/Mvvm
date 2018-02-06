namespace Nito.Mvvm
{
    /// <summary>
    /// Contract for cancellable asynchronous command
    /// </summary>
    public interface ICancellableAsyncCommand : IAsyncCommand
    {
        /// <summary>
        /// Command to cancel corresponding <see cref="IAsyncCommand"/>
        /// </summary>
        CancelCommand CancelCommand { get; }

        /// <summary>
        /// Method for cancelling corresponding <see cref="IAsyncCommand"/>
        /// </summary>
        void Cancel();
    }
}
