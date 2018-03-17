namespace Sharding
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.WindowsAzure.Storage.Blob;

    public static class ShardData
    {
        [FunctionName("ShardData")]
        public static async Task Run([BlobTrigger("olympic-data/{name}", Connection = "AzureWebJobsStorage")]Stream blob, 
                                     string name,
                                     Binder binder,
                                     TraceWriter log)
        {
            var recordsByYear = await GetRecordsByYear(blob);
            await WriteFilePerYear(binder, recordsByYear);
        }

        private static async Task<Dictionary<string, List<string>>> GetRecordsByYear(Stream blob)
        {
            var recordsByYear = new Dictionary<string, List<string>>();
            using (var sr = new StreamReader(blob))
            {
                while (!sr.EndOfStream)
                {
                    var record = await sr.ReadLineAsync();
                    var year = record.Substring(0, 4);
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

        private static async Task WriteFilePerYear(IBinder binder, Dictionary<string, List<string>> recordsByYear)
        {
            foreach (var entry in recordsByYear)
            {
                var outputBlob = await binder.BindAsync<CloudBlockBlob>(new BlobAttribute($"olympic-data-by-year/{entry.Key}.csv"));
                outputBlob.Properties.ContentType = "text/csv";
                await outputBlob.UploadTextAsync(string.Join(Environment.NewLine, entry.Value));
            }
        }
    }
}
