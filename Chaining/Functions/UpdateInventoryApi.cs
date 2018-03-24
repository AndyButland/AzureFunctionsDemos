namespace Chaining.Functions
{
    using System.Threading.Tasks;
    using Common.Chaining;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    public static class UpdateInventoryApi
    {
        [FunctionName("UpdateInventoryApi")]
        public static async Task Run(
            [QueueTrigger("chaining-2", Connection = "AzureWebJobsStorage")]string item,
            [Queue("chaining-3", Connection = "AzureWebJobsStorage")] IAsyncCollector<OrderDetail> outputQueue,
            TraceWriter log)
        {
            var orderDetail = JsonConvert.DeserializeObject<OrderDetail>(item);

            if (await UpdateInventory(orderDetail))
            {
                await outputQueue.AddAsync(orderDetail);
            }
        }

        private static async Task<bool> UpdateInventory(OrderDetail orderDetail)
        {
            return await Task.FromResult(true);
        }
    }
}
