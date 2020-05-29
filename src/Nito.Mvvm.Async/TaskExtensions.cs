namespace Nito.Mvvm
{
    using System.Threading.Tasks;

    internal static class TaskExt
    {
        private static Task s_completedTask;

        internal static Task CompletedTask
        {
            get
            {
                var completedTask = s_completedTask;
                if (completedTask == null)
                    s_completedTask = Task.FromResult(false);
                return s_completedTask;
            }
        }
    }
}
