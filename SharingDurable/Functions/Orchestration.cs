namespace Sharding.Durable.Functions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Sharding;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    public static class Orchestration
    {
        [FunctionName("Orchestration")]
        public static async Task Run([OrchestrationTrigger]DurableOrchestrationContext ctx,
                                     TraceWriter log)
        {
            var input = ctx.GetInput<string>();
            var recordsByYear = DecompressAndDeserialize(input);

            var parallelTasks = new List<Task<Dictionary<string, MedalCount>>>();
            foreach (var record in recordsByYear)
            {
                var task = ctx.CallActivityAsync<Dictionary<string, MedalCount>>("ProcessDataForYear", record.Value);
                parallelTasks.Add(task);
            }

            await Task.WhenAll(parallelTasks);

            await ctx.CallActivityAsync<IEnumerable<Dictionary<string, MedalCount>>>("WriteOutput", parallelTasks.Select(x => x.Result));
        }

        private static Dictionary<string, List<string>> DecompressAndDeserialize(string input)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(StringCompressor.DecompressString(input));
        }
    }
}
