namespace Retry.Functions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class SimpleRetry
    {
        private static readonly string StorageConnectionString = EnvironmentVariables.GetValue("AzureWebJobsStorage");
        private static readonly string LogTableName = EnvironmentVariables.GetValue("SimpleRetryLogTableName");

        [FunctionName("SimpleRetry")]
        public static async Task Run([QueueTrigger("retry-demo", Connection = "AzureWebJobsStorage")]string item,
                               int dequeueCount,
                               TraceWriter log)
        {
            if (!MessageHelper.TryDeserializeMessage(item, out Message message))
            {
                // Could not deserialize message - no amount of retries will help with this, so treat as fatal error.
                log.Error("Message could not be derserialized.");
            }
            else
            {
                log.Info($"Processing message with Id: {message.Id}. Dequeue count: {dequeueCount}.");

                var result = MessageHelper.PerformOperation(message);

                await StorageHelper.LogMessageResult(message.Id, dequeueCount, dequeueCount * 5, result,
                    StorageConnectionString, LogTableName);

                switch (result)
                {
                    case OperationResult.Success:
                        log.Info("Message sucessfully processed.");
                        break;
                    case OperationResult.FailFatal:
                        log.Error("Message failed with fatal error.");
                        break;
                    case OperationResult.FailCanRetry:
                        var errorMessage = "Message with failed with tranisent error. Putting message back on queue for retrying";
                        log.Warning(errorMessage);

                        // To put the message back on the queue, throw an arbitrary exception.
                        throw new InvalidOperationException(errorMessage);
                }
            }
        }
    }
}
