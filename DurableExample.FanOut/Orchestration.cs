namespace DurableExample.FanOut
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.WindowsAzure.Storage.Blob;

    public static class Orchestration
    {
        [FunctionName("SumNumbers")]
        public static async Task SumNumbers([OrchestrationTrigger]DurableOrchestrationContext context)
        {
            var number = context.GetInput<int>();

            var parallelTasks = new List<Task<int>>();

            for (var i = 0; i < number; i++)
            {
                var task = context.CallActivityAsync<int>("SquareNumber", i);
                parallelTasks.Add(task);
            }

            await Task.WhenAll(parallelTasks);

            var sum = parallelTasks.Sum(t => t.Result);
            await context.CallActivityAsync("OutputResult", sum);
        }

        [FunctionName("SquareNumber")]
        public static int SquareNumber([ActivityTrigger]DurableActivityContext context)
        {
            var number = context.GetInput<int>();
            return number * number;
        }

        [FunctionName("OutputResult")]
        public static async Task OutputResult([ActivityTrigger]DurableActivityContext context,
                                              [Blob("durable-example-results/results.txt", Connection = "AzureWebJobsStorage")]CloudBlockBlob outputBlob)
        {
            var sum = context.GetInput<int>();
            outputBlob.Properties.ContentType = "text/plain";
            await outputBlob.UploadTextAsync(sum.ToString());
        }
    }
}
