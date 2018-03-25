namespace Chaining.Durable.Functions
{
    using System;
    using System.Threading.Tasks;
    using Common.Chaining;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using SendGrid.Helpers.Mail;

    public static class Orchestration
    {
        [FunctionName("Orchestration")]
        public static async Task Run(
            [OrchestrationTrigger]DurableOrchestrationContext ctx,
            TraceWriter log)
        {
            var input = ctx.GetInput<string>();
            var orderDetail = JsonConvert.DeserializeObject<OrderDetail>(input);

            await ctx.CallActivityAsync("WriteToDatabase", orderDetail);
            if (await ctx.CallActivityAsync<bool>("UpdateInventoryApi", orderDetail))
            {
                await ctx.CallActivityAsync<OrderDetail>("SendEmail", orderDetail);
            }
        }

        [FunctionName("WriteToDatabase")]
        public static async Task WriteToDatabase(
            [ActivityTrigger]DurableActivityContext ctx)
        {
            var orderDetail = ctx.GetInput<OrderDetail>();
            orderDetail.Id = Guid.NewGuid().ToString();
            await StorageHelper.SaveOrder(orderDetail);
        }

        [FunctionName("UpdateInventoryApi")]
        public static async Task<bool> UpdateInventoryApi(
            [ActivityTrigger] DurableActivityContext ctx)
        {
            var orderDetail = ctx.GetInput<OrderDetail>();
            return await UpdateInventory(orderDetail);
        }

        private static async Task<bool> UpdateInventory(OrderDetail orderDetail)
        {
            return await Task.FromResult(true);
        }

        [FunctionName("SendEmail")]
        public static void SendEmail(
            [ActivityTrigger] DurableActivityContext ctx,
            [SendGrid] out SendGridMessage message)
        {
            var orderDetail = ctx.GetInput<OrderDetail>();

            message = new SendGridMessage();
            message.AddTo(orderDetail.CustomerEmail);
            message.AddContent("text/html", $"<p>Hi {orderDetail.CustomerName}. Thanks for your order of a {orderDetail.ProductName}. It's on it's way.</p>");
            message.SetFrom(new EmailAddress("noreply@exampleco.com"));
            message.SetSubject("Order confirmation");
        }
    }
}
