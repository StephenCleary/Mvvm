using System.Threading.Tasks;

namespace Nito.Mvvm
{
    /// <summary>
    /// A strongly typed async version of <see cref="IAsyncCommand"/>.
    /// </summary>
    public interface IAsyncCommand<T> : IAsyncCommand
    {
        /// <summary>
        /// Executes the asynchronous command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        Task ExecuteAsync(T parameter);
    }
}