namespace Timer.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Timer.Models;

    public static class GetNotifications
    {
        private const string CronExpressionEveryMinute = "0 */1 * * * *";
        private const string CronExpressionSevenAmUtc = "0 0 7 * * *";

        [FunctionName("GetNotifications")]
        public static async Task Run(
            [TimerTrigger(CronExpressionEveryMinute)]TimerInfo myTimer,
            [Queue("event-notifications", Connection = "AzureWebJobsStorage")] IAsyncCollector<EmailDetail> outputQueue,
            TraceWriter log)
        {
            var notifications = GetEventNotifications();
            foreach (var notification in notifications)
            {
                var emailDetail = CreateEmailDetail(notification);
                await outputQueue.AddAsync(emailDetail);
            }
        }

        private static IEnumerable<EventDetail> GetEventNotifications()
        {
            return new List<EventDetail>
                {
                    new EventDetail
                        {
                            ParticipantName = "Andy Butland",
                            ParticipantEmail = "test@test.com",
                            EventName = "Tech Conference",
                            Location = "Copenhagen",
                            StartDateTime = DateTime.Now.Date.AddDays(1).AddHours(10),
                        }
                };
        }

        private static EmailDetail CreateEmailDetail(EventDetail notification)
        {
            return new EmailDetail
            {
                From = "noreply@myevents.com",
                To = notification.ParticipantEmail,
                Subject = $"Reminder for tomorow's event: {notification.EventName}",
                Body = $"<p>Don't forget {notification.EventName} starting at {notification.StartDateTime.ToLongTimeString()} tomorrow!</p>"
            };
        }
    }
}
