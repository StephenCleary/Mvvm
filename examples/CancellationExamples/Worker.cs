using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancellationExamples
{
    public static class Worker
    {
        public static async Task<int> DoWorkAsync(int id, IProgress<string> progress = null)
        {
            for (int i = 0; i != 5; ++i)
            {
                progress?.Report($"{id} Doing work {i}...");
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
            progress?.Report($"{id} Finishing...");
            return id;
        }
    }
}
