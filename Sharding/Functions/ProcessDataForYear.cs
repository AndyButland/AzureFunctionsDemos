namespace Sharding.Functions
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using CsvHelper;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class ProcessDataForYear
    {
        [FunctionName("ProcessDataForYear")]
        public static async Task Run([BlobTrigger("olympic-data-by-year/{name}", Connection = "AzureWebJobsStorage")]Stream blob, 
                                     string name,
                                     [Queue("olympic-data", Connection = "AzureWebJobsStorage")] IAsyncCollector<string> outputQueue,
                                     TraceWriter log)
        {
            var medalsPerCountry = GetMedalsPerCountry(blob);

            var year = name.Replace(".csv", string.Empty);
            await StorageHelper.UpdateYearRecords(year, medalsPerCountry);

            await outputQueue.AddAsync(year);
        }

        private static Dictionary<string, MedalCount> GetMedalsPerCountry(Stream blob)
        {
            var medalsPerCountry = new Dictionary<string, MedalCount>();
            var csv = new CsvReader(new StreamReader(blob));
            var records = csv.GetRecords<RecordDetail>();

            foreach (var record in records)
            {
                AddOrUpdateMedalDetailPerCountry(medalsPerCountry, record);
            }

            return medalsPerCountry;
        }

        private static void AddOrUpdateMedalDetailPerCountry(Dictionary<string, MedalCount> medalsPerCountry, RecordDetail record)
        {
            if (medalsPerCountry.ContainsKey(record.Country))
            {
                switch (record.Medal)
                {
                    case "Gold":
                        medalsPerCountry[record.Country].Gold++;
                        break;
                    case "Silver":
                        medalsPerCountry[record.Country].Silver++;
                        break;
                    case "Bronze":
                        medalsPerCountry[record.Country].Bronze++;
                        break;
                }
            }
            else
            {
                medalsPerCountry.Add(
                    record.Country,
                    new MedalCount
                        {
                            Gold = record.Medal == "Gold" ? 1 : 0,
                            Silver = record.Medal == "Silver" ? 1 : 0,
                            Bronze = record.Medal == "Bronze" ? 1 : 0
                        });
            }
        }

        public class RecordDetail
        {
            public string City { get; set; }

            public string Discipline { get; set; }

            public string Athlete { get; set; }

            public string Country { get; set; }

            public string Gender { get; set; }

            public string Event { get; set; }

            public string Medal { get; set; }
        }
    }
}
