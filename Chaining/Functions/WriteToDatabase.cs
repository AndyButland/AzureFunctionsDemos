namespace Chaining.Functions
{
    using System;
    using System.Threading.Tasks;
    using Common.Chaining;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;

    public static class WriteToDatabase
    {
        [FunctionName("WriteToDatabase")]
        public static async Task Run(
            [QueueTrigger("chaining-1", Connection = "AzureWebJobsStorage")]string item,
            [Queue("chaining-2", Connection = "AzureWebJobsStorage")] IAsyncCollector<OrderDetail> outputQueue,
            TraceWriter log)
        {
            var orderDetail = JsonConvert.DeserializeObject<OrderDetail>(item);
            orderDetail.Id = Guid.NewGuid().ToString();
            await StorageHelper.SaveOrder(orderDetail);
            await outputQueue.AddAsync(orderDetail);
        }
    }
}
