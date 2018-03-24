namespace Chaining.Functions
{
    using Common.Chaining;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using SendGrid.Helpers.Mail;

    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static void Run(
            [QueueTrigger("chaining-3", Connection = "AzureWebJobsStorage")]string item,
            [SendGrid] out SendGridMessage message,
            TraceWriter log)
        {
            var orderDetail = JsonConvert.DeserializeObject<OrderDetail>(item);

            message = new SendGridMessage();
            message.AddTo(orderDetail.CustomerEmail);
            message.AddContent("text/html", $"<p>Hi {orderDetail.CustomerName}. Thanks for your order of a {orderDetail.ProductName}. It's on it's way.</p>");
            message.SetFrom(new EmailAddress("noreply@exampleco.com"));
            message.SetSubject("Order confirmation");
        }
    }
}
