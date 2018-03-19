namespace Common.Sharding
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    using Newtonsoft.Json;

    public static class StorageHelper
    {
        private static readonly string StorageConnectionString = EnvironmentVariables.GetValue("AzureWebJobsStorage");
        private static readonly string TableName = EnvironmentVariables.GetValue("ResultsTableName");

        public static async Task CreateYearRecords(IEnumerable<string> years)
        {
            var table = await CreateTable();
            var operation = new TableBatchOperation();

            foreach (var year in years)
            {
                var record = new ResultRecord
                    {
                        PartitionKey = "Summer",
                        RowKey = year
                    };
                operation.Insert(record);
            }

            await table.ExecuteBatchAsync(operation);
        }

        public static async Task UpdateYearRecords(string year, Dictionary<string, MedalCount> results)
        {
            var table = await GetTable();
            var record = new ResultRecord
                {
                    PartitionKey = "Summer",
                    RowKey = year,
                    Results = JsonConvert.SerializeObject(results),
                    ETag = "*"  // ensure "last one wins" is used for concurrency
                };
            var operation = TableOperation.Replace(record);
            await table.ExecuteAsync(operation);
        }

        public static async Task<Dictionary<string, Dictionary<string, MedalCount>>> GetYearRecords()
        {
            var table = await GetTable();
            var query = new TableQuery<ResultRecord>();
            var results = table.ExecuteQuery(query).ToList();
            return results
                .Select(x => new KeyValuePair<string, Dictionary<string, MedalCount>>(
                    x.RowKey, 
                    string.IsNullOrEmpty(x.Results)
                        ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, MedalCount>>(x.Results)))
                .ToDictionary(t => t.Key, t => t.Value);
        }

        private static async Task<CloudTable> CreateTable()
        {
            var account = GetStorageAccount();

            var client = account.CreateCloudTableClient();

            var table = client.GetTableReference(TableName);
            await table.DeleteIfExistsAsync();
            await table.CreateAsync();
            return table;
        }

        private static async Task<CloudTable> GetTable()
        {
            var account = GetStorageAccount();

            var client = account.CreateCloudTableClient();

            return client.GetTableReference(TableName);
        }

        private static CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(StorageConnectionString);
        }
    }
}
