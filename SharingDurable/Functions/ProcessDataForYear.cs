namespace Sharding.Durable.Functions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Common.Sharding;
    using CsvHelper;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class ProcessDataForYear
    {
        [FunctionName("ProcessDataForYear")]
        public static Task<Dictionary<string, MedalCount>> Run([ActivityTrigger]List<string> records, TraceWriter log)
        {
            return Task.FromResult(GetMedalsPerCountry(records));
        }

        private static Dictionary<string, MedalCount> GetMedalsPerCountry(IEnumerable<string> recordLines)
        {
            var medalsPerCountry = new Dictionary<string, MedalCount>();
            var csv = new CsvReader(new StringReader(string.Join(Environment.NewLine, recordLines)));
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
    }
}
