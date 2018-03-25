namespace Sharding.Durable.Functions
{
    using System;
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
            var recordsByYear = DecompressAndDeserializeFunctionInput(input);
            Console.WriteLine($"Processing input for {recordsByYear.Count} years.");

            var parallelTasks = new List<Task<string>>();
            foreach (var record in recordsByYear)
            {
                var inputToActivity = SerializeAndCompress(record.Value);
                var task = ctx.CallActivityAsync<string>("ProcessDataForYear", inputToActivity);
                parallelTasks.Add(task);
            }

            await Task.WhenAll(parallelTasks);

            await ctx.CallActivityAsync<IEnumerable<string>>("WriteOutput", parallelTasks.Select(x => x.Result));
        }

        private static Dictionary<string, List<string>> DecompressAndDeserializeFunctionInput(string input)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(StringCompressor.DecompressString(input));
        }

        private static string SerializeAndCompress(IEnumerable<string> recordsForYear)
        {
            return StringCompressor.CompressString(JsonConvert.SerializeObject(recordsForYear));
        }
    }
}
