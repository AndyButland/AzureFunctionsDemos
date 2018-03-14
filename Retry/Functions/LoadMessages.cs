namespace Retry.Functions
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class LoadMessages
    {
        [FunctionName("LoadMessages")]
        public static async Task Run([QueueTrigger("retry-demo-load", Connection = "AzureWebJobsStorage")]string item,
                                     TraceWriter log,
                                     [Queue("retry-demo", Connection = "AzureWebJobsStorage")] IAsyncCollector<Message> outputQueue)
        {
            const int NumberOfMessages = 50;
            for (var i = 0; i < NumberOfMessages; i++)
            {
                var message = CreateMessage(i + 1);
                await outputQueue.AddAsync(message);
            }
        }

        private static Message CreateMessage(int id)
        {
            return new Message { Id = id, Content = $"Message {id}" };
        }
    }
}
