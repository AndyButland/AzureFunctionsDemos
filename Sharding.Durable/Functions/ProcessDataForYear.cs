namespace Sharding.Durable.Functions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Common;
    using Common.Sharding;
    using CsvHelper;
    using Microsoft.Azure.WebJobs;
    using Newtonsoft.Json;

    public static class ProcessDataForYear
    {
        [FunctionName("ProcessDataForYear")]
        public static string Run([ActivityTrigger]DurableActivityContext ctx)
        {
            var input = ctx.GetInput<string>();
            var recordsForYear = DecompressAndDeserialize(input);
            return SerializeAndCompress(GetMedalsPerCountry(recordsForYear));
        }

        private static IEnumerable<string> DecompressAndDeserialize(string input)
        {
            return JsonConvert.DeserializeObject<IEnumerable<string>>(StringCompressor.DecompressString(input));
        }

        private static Dictionary<string, MedalCount> GetMedalsPerCountry(IEnumerable<string> recordLines)
        {
            var medalsPerCountry = new Dictionary<string, MedalCount>();
            var csv = new StringReader("Country,Medal" +
                                       Environment.NewLine +
                                       string.Join(Environment.NewLine, recordLines));
            var csvReader = new CsvReader(csv);
            var records = csvReader.GetRecords<RecordDetailLite>();

            foreach (var record in records)
            {
                AddOrUpdateMedalDetailPerCountry(medalsPerCountry, record);
            }

            return medalsPerCountry;
        }

        private static void AddOrUpdateMedalDetailPerCountry(Dictionary<string, MedalCount> medalsPerCountry, RecordDetailLite record)
        {
            if (medalsPerCountry.ContainsKey(record.Country))
            {
                switch (record.Medal)
                {
                    case "G":
                        medalsPerCountry[record.Country].Gold++;
                        break;
                    case "S":
                        medalsPerCountry[record.Country].Silver++;
                        break;
                    case "B":
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
                            Gold = record.Medal == "G" ? 1 : 0,
                            Silver = record.Medal == "S" ? 1 : 0,
                            Bronze = record.Medal == "B" ? 1 : 0
                        });
            }
        }

        private static string SerializeAndCompress(Dictionary<string, MedalCount> results)
        {
            return StringCompressor.CompressString(JsonConvert.SerializeObject(results));
        }
    }
}
