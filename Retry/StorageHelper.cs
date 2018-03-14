namespace Retry
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    public static class StorageHelper
    {
        public static async Task LogMessageResult(int messageId, int attempt, int delay, OperationResult result, 
                                           string storageConnection, string tableName)
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

            var table = await GetOrCreateTable(storageConnection, tableName);
            var operation = TableOperation.Insert(record);
            await table.ExecuteAsync(operation);
        }

        public static async Task SaveMessageDequeueCount(int messageId, int attempt,
                                                         string storageConnection, string tableName)
        {
            var record = new MessageStatusRecord
                {
                    PartitionKey = "RetryDemo",
                    RowKey = messageId.ToString(),
                    DequeueCount = attempt
                };

            var table = await GetOrCreateTable(storageConnection, tableName);
            var operation = TableOperation.InsertOrReplace(record);
            await table.ExecuteAsync(operation);
        }

        public static async Task<int> GetMessageDequeueCount(int messageId, string storageConnection, string tableName)
        {
            var table = await GetOrCreateTable(storageConnection, tableName);

            var operation = TableOperation.Retrieve<MessageStatusRecord>("RetryDemo", messageId.ToString());
            var result = await table.ExecuteAsync(operation);
            var typedResult = result?.Result as MessageStatusRecord;

            return typedResult?.DequeueCount ?? 1;
        }

        public static async Task AddToQueue(string messageContent, TimeSpan visibilityDelay, string storageConnection, string queueName)
        {
            var queue = await GetOrCreateQueue(storageConnection, queueName);

            var message = new CloudQueueMessage(messageContent);
            await queue.AddMessageAsync(message, null, visibilityDelay, null, null);
        }

        private static async Task<CloudTable> GetOrCreateTable(string storageConnection, string tableName)
        {
            var account = GetStorageAccount(storageConnection);

            var client = account.CreateCloudTableClient();

            var table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        private static CloudStorageAccount GetStorageAccount(string storageConnection)
        {
            return CloudStorageAccount.Parse(storageConnection);
        }

        private static async Task<CloudQueue> GetOrCreateQueue(string storageConnection, string queueName)
        {
            var queue = GetQueueReference(storageConnection, queueName);
            await queue.CreateIfNotExistsAsync();
            return queue;
        }

        private static CloudQueue GetQueueReference(string storageConnection, string queueName)
        {
            var account = GetStorageAccount(storageConnection);
            var client = account.CreateCloudQueueClient();
            return client.GetQueueReference(queueName);
        }
    }
}
