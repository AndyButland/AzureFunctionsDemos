namespace DurableExample
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class LoadMessages
    {
        [FunctionName("Start")]
        public static async Task Run([QueueTrigger("durable-example-start", Connection = "AzureWebJobsStorage")]string item,
                                     [OrchestrationClient] DurableOrchestrationClient starter, 
                                     TraceWriter log)
        {
            await starter.StartNewAsync("HelloWorld", "Andy");
        }
    }
}
