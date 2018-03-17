namespace Sharding.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Sharding;

    public static class ProcessDataForYearComplete
    {
        [FunctionName("ProcessDataForYearComplete")]
        public static async Task Run([QueueTrigger("olympic-data", Connection = "AzureWebJobsStorage")]string item,
                                     [Blob("olympic-data-results/results.csv", Connection = "AzureWebJobsStorage")]CloudBlockBlob outputBlob,
                                     TraceWriter log)
        {
            var records = await StorageHelper.GetYearRecords();
            if (records.Any(x => x.Value == null))
            {
                log.Info("Processing for all years not yet complete");
                return;
            }

            log.Info("Processing for all years complete");

            var overallResults = GetOverallResults(records);

            outputBlob.Properties.ContentType = "text/csv";
            await outputBlob.UploadTextAsync(
                GetHeaders() + 
                Environment.NewLine + 
                string.Join(Environment.NewLine,
                    GetOrderedResults(overallResults)));
        }

        private static Dictionary<string, MedalCount> GetOverallResults(Dictionary<string, Dictionary<string, MedalCount>> records)
        {
            var results = new Dictionary<string, MedalCount>();
            foreach (var yearEntry in records)
            {
                foreach (var countryEntry in yearEntry.Value)
                {
                    if (results.ContainsKey(countryEntry.Key))
                    {
                        results[countryEntry.Key].Gold += countryEntry.Value.Gold;
                        results[countryEntry.Key].Silver += countryEntry.Value.Silver;
                        results[countryEntry.Key].Bronze += countryEntry.Value.Bronze;
                    }
                    else
                    {
                        results.Add(countryEntry.Key, countryEntry.Value);
                    }
                }
            }

            return results;
        }

        private static string GetHeaders()
        {
            return "Country,Golds,Silver,Bronze";
        }

        private static IEnumerable<string> GetOrderedResults(Dictionary<string, MedalCount> overallResults)
        {
            return overallResults
                .OrderByDescending(x => x.Value.Gold)
                .ThenByDescending(x => x.Value.Silver)
                .ThenByDescending(x => x.Value.Bronze)
                .Select(x => $"{x.Key},{x.Value.Gold},{x.Value.Silver},{x.Value.Bronze}");
        }
    }
}
