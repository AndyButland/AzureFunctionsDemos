namespace Retry
{
    using System;
    using Newtonsoft.Json;

    public static class MessageHelper
    {
        public static bool TryDeserializeMessage(string item, out Message message)
        {
            try
            {
                message = JsonConvert.DeserializeObject<Message>(item);
                return true;
            }
            catch (Exception)
            {
                message = null;
                return false;
            }
        }

        public static OperationResult PerformOperation(Message message)
        {
            var rnd = new Random();
            var randomNumber = rnd.Next(0, 10);

            // Simulate transient error occured 20% of the time - we can can retry from this
            if (randomNumber <= 1)
            {
                return OperationResult.FailCanRetry;
            }

            // Simulate fatal error occured 10% of the time - we can't retry from this
            if (randomNumber == 9)
            {
                return OperationResult.FailFatal;
            }

            // Otherwise simulate success
            return OperationResult.Success;
        }
    }
}
