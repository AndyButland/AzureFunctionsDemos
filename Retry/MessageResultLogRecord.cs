namespace Retry
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class MessageResultLogRecord : TableEntity
    {
        public int MessageId { get; set; }

        public int Attempt { get; set; }

        public int Delay { get; set; }

        public string Result { get; set; }
    }
}
