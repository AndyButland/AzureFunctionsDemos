namespace Timer.Functions
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Newtonsoft.Json;
    using SendGrid.Helpers.Mail;
    using Timer.Models;

    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static void Run(
            [QueueTrigger("event-notifications", Connection = "AzureWebJobsStorage")]string queueItem,
            [SendGrid] out SendGridMessage message,
            TraceWriter log)
        {
            var email = JsonConvert.DeserializeObject<EmailDetail>(queueItem);

            message = new SendGridMessage();
            message.AddTo(email.To);
            message.AddContent("text/html", email.Body);
            message.SetFrom(new EmailAddress(email.From));
            message.SetSubject(email.Subject);
        }
    }
}
