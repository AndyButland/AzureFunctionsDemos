namespace Common.Chaining
{
    using System.Threading.Tasks;
    using Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public static class StorageHelper
    {
        private static readonly string StorageConnectionString = EnvironmentVariables.GetValue("AzureWebJobsStorage");
        private static readonly string TableName = EnvironmentVariables.GetValue("OrdersTableName");

        public static async Task SaveOrder(OrderDetail orderDetail)
        {
            var table = await GetOrCreateTable();
            var record = new OrderRecord
                {
                    PartitionKey = orderDetail.Id,
                    RowKey = string.Empty,
                    CustomerName = orderDetail.CustomerName,
                    ProductName = orderDetail.ProductName,
                    Quantity = orderDetail.Quantity
                };
            var operation = TableOperation.Insert(record);
            await table.ExecuteAsync(operation);
        }
        
        private static async Task<CloudTable> GetOrCreateTable()
        {
            var account = GetStorageAccount();

            var client = account.CreateCloudTableClient();

            var table = client.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        private static CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(StorageConnectionString);
        }
    }
}
