namespace Retry.Functions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class DelayedRetry
    {
        private const string QueueName = "delayed-retry-demo";
        private static readonly string LogTableName = EnvironmentVariables.GetValue("DelayedRetryLogTableName");
        private static readonly string MessageStatusTableName = EnvironmentVariables.GetValue("DelayedRetryMessageStatusTableName");

        [FunctionName("DelayedRetry")]
        public static async Task Run([QueueTrigger(QueueName, Connection = "AzureWebJobsStorage")]string item,
                                     TraceWriter log)
        {
            if (!MessageHelper.TryDeserializeMessage(item, out Message message))
            {
                // Could not deserialize message - no amount of retries will help with this, so treat as fatal error.
                log.Error("Message could not be derserialized.");
            }
            else
            {
                var maxRetries = int.Parse(EnvironmentVariables.GetValue("MaxRetries"));
                var visibilityTimeouts = EnvironmentVariables.GetValue("VisibilityTimeoutsInSeconds")
                    .Split(',')
                    .Select(int.Parse)
                    .ToArray();

                var dequeueCount = await StorageHelper.GetMessageDequeueCount(MessageStatusTableName, message.Id);
                log.Info($"Processing message with Id: {message.Id}. Dequeue count: {dequeueCount}.");

                var result = MessageHelper.PerformOperation(message);

                var delay = visibilityTimeouts.Take(dequeueCount - 1).Sum();
                await StorageHelper.LogMessageResult(LogTableName, message.Id, dequeueCount, delay, result);

                switch (result)
                {
                    case OperationResult.Success:
                        log.Info("Message sucessfully processed.");
                        break;
                    case OperationResult.FailFatal:
                        log.Error("Message failed with fatal error.");
                        break;
                    case OperationResult.FailCanRetry:

                        // Allow message to process BUT if not exceeded maximum retries, put back on the queue with an increasing delay.
                        if (dequeueCount >= maxRetries)
                        {
                            log.Error("Message with failed with tranisent error but maximum number of retries has been met.");
                        }
                        else
                        {
                            log.Warning("Message with failed with tranisent error. Putting message back on queue for retrying");

                            await StorageHelper.SaveMessageDequeueCount(MessageStatusTableName, message.Id, dequeueCount + 1);

                            await StorageHelper.AddToQueue(QueueName, item, TimeSpan.FromSeconds(visibilityTimeouts[dequeueCount - 1]));
                        }

                        break;
                }
            }
        }
    }
}
