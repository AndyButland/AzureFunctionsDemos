namespace Sharding.Durable.Functions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Common;
    using Common.Sharding;
    using CsvHelper;
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
            var recordsByYear = GetRecordsByYear(blob);
            var inputToOrchestration = SerializeAndCompress(recordsByYear);
            await starter.StartNewAsync("Orchestration", inputToOrchestration);
        }

        private static Dictionary<string, List<string>> GetRecordsByYear(Stream blob)
        {
            var recordsByYear = new Dictionary<string, List<string>>();

            var csv = new CsvReader(new StreamReader(blob));
            var records = csv.GetRecords<RecordDetail>();

            foreach (var record in records)
            {
                // Information transfer between functions goes via Azure storage queues which has a 64KB 
                // size limit.  As UTF-16 is used, means a string must be less that 32KB.
                // For the purposes of demonstration, we'll just shave off a few years to get under the limit.
                if (int.Parse(record.Year) >= 1920)
                {
                    AddRecordToYear(recordsByYear, record);
                }
            }

            return recordsByYear;
        }

        private static void AddRecordToYear(IDictionary<string, List<string>> recordsByYear, RecordDetail record)
        {
            var recordLite = $"{record.Country},{record.Medal.Substring(0, 1)}";
            if (recordsByYear.ContainsKey(record.Year))
            {
                recordsByYear[record.Year].Add(recordLite);
            }
            else
            {
                recordsByYear.Add(record.Year, new List<string> { recordLite });
            }
        }

        private static string SerializeAndCompress(Dictionary<string, List<string>> recordsByYear)
        {
            return StringCompressor.CompressString(JsonConvert.SerializeObject(recordsByYear));
        }
    }
}
