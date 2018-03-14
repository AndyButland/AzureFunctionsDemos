namespace Retry
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class MessageStatusRecord : TableEntity
    {
        public int DequeueCount { get; set; }
    }
}
