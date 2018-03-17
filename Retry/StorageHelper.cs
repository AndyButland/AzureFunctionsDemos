namespace Retry
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    public static class StorageHelper
    {
        private static readonly string StorageConnectionString = EnvironmentVariables.GetValue("AzureWebJobsStorage");

        public static async Task LogMessageResult(string tableName, int messageId, int attempt, int delay, OperationResult result)
        {
            var record = new MessageResultLogRecord
                {
                    PartitionKey = "RetryDemo",
                    RowKey = Guid.NewGuid().ToString(),
                    MessageId = messageId,
                    Attempt = attempt,
                    Delay = delay,
                    Result = result.ToString()
                };

            var table = await GetOrCreateTable(tableName);
            var operation = TableOperation.Insert(record);
            await table.ExecuteAsync(operation);
        }

        public static async Task SaveMessageDequeueCount(string tableName, int messageId, int attempt)
        {
            var record = new MessageStatusRecord
                {
                    PartitionKey = "RetryDemo",
                    RowKey = messageId.ToString(),
                    DequeueCount = attempt
                };

            var table = await GetOrCreateTable(tableName);
            var operation = TableOperation.InsertOrReplace(record);
            await table.ExecuteAsync(operation);
        }

        public static async Task<int> GetMessageDequeueCount(string tableName, int messageId)
        {
            var table = await GetOrCreateTable(tableName);

            var operation = TableOperation.Retrieve<MessageStatusRecord>("RetryDemo", messageId.ToString());
            var result = await table.ExecuteAsync(operation);
            var typedResult = result?.Result as MessageStatusRecord;

            return typedResult?.DequeueCount ?? 1;
        }

        public static async Task AddToQueue(string queueName, string messageContent, TimeSpan visibilityDelay)
        {
            var queue = await GetOrCreateQueue(queueName);

            var message = new CloudQueueMessage(messageContent);
            await queue.AddMessageAsync(message, null, visibilityDelay, null, null);
        }

        private static async Task<CloudTable> GetOrCreateTable(string tableName)
        {
            var account = GetStorageAccount();

            var client = account.CreateCloudTableClient();

            var table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        private static CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(StorageConnectionString);
        }

        private static async Task<CloudQueue> GetOrCreateQueue(string queueName)
        {
            var queue = GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        private static CloudQueue GetQueueReference(string queueName)
        {
            var account = GetStorageAccount();
            var client = account.CreateCloudQueueClient();
            return client.GetQueueReference(queueName);
        }
    }
}
