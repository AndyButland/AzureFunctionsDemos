namespace Chaining.Durable.Functions
{
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    public static class StartOrchestration
    {
        [FunctionName("StartOrchestration")]
        public static async Task Run(
            [QueueTrigger("chaining-1", Connection = "AzureWebJobsStorage")]string queueItem, 
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {
            await starter.StartNewAsync("Orchestration", queueItem);
        }
    }
}
