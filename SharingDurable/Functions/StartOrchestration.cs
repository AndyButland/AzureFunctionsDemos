namespace Sharding.Durable.Functions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    public static class StartOrchestration
    {
        [FunctionName("StartOrchestration")]
        public static async Task Run([BlobTrigger("olympic-data/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
                                     [OrchestrationClient] DurableOrchestrationClient starter,
                                     TraceWriter log)
        {
            var recordsByYear = await GetRecordsByYear(blob);
            var inputToOrchestration = SerializeAndCompress(recordsByYear);
            await starter.StartNewAsync("Orchestration", inputToOrchestration);
        }

        private static async Task<Dictionary<string, List<string>>> GetRecordsByYear(Stream blob)
        {
            var recordsByYear = new Dictionary<string, List<string>>();
            using (var sr = new StreamReader(blob))
            {
                // Skip header row
                await sr.ReadLineAsync();

                while (!sr.EndOfStream)
                {
                    var record = await sr.ReadLineAsync();
                    var year = record.Substring(0, 4);

                    // TODO: truncate to just country and medal using CSV helper
                    var restOfRecord = record.Substring(5);

                    AddRecordToYear(recordsByYear, year, restOfRecord);
                }
            }

            return recordsByYear;
        }

        private static void AddRecordToYear(IDictionary<string, List<string>> recordsByYear, string year, string restOfRecord)
        {
            if (recordsByYear.ContainsKey(year))
            {
                recordsByYear[year].Add(restOfRecord);
            }
            else
            {
                recordsByYear.Add(year, new List<string> { restOfRecord });
            }
        }

        private static string SerializeAndCompress(Dictionary<string, List<string>> recordsByYear)
        {
            return StringCompressor.CompressString(JsonConvert.SerializeObject(recordsByYear));
        }
    }
}
