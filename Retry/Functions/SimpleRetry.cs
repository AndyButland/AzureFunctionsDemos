namespace Retry.Functions
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    public static class SimpleRetry
    {
        [FunctionName("SimpleRetry")]
        public static void Run([QueueTrigger("retry-demo", Connection = "AzureWebJobsStorage")]string item,
                               int dequeueCount,
                               TraceWriter log)
        {
            Message message;

            if (!TryDeserializeMessage(item, out message))
            {
                // Could not deserialize message - no amount of retries will help with this, so treat as fatal error.
                log.Error("Message could not be derserialized.");
            }
            else
            {
                log.Info($"Processing message with Id: {message.Id}. Dequeue count: {dequeueCount}.");
                var result = PerformOperation(message);

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

        private static bool TryDeserializeMessage(string item, out Message message)
        {
            try
            {
                message = JsonConvert.DeserializeObject<Message>(item);
                return true;
            }
            catch (Exception)
            {
                message = null;
                return false;
            }
        }

        private static OperationResult PerformOperation(Message message)
        {
            var rnd = new Random();
            var randomNumber = rnd.Next(0, 10);

            // Simulate transient error occured 20% of the time - we can can retry from this
            if (randomNumber <= 1)
            {
                return OperationResult.FailCanRetry;
            }

            // Simulate fatal error occured 10% of the time - we can't retry from this
            if (randomNumber == 9)
            {
                return OperationResult.FailFatal;
            }

            // Otherwise simulate success
            return OperationResult.Success;
        }
    }
}
