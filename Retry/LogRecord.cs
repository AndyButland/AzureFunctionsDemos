namespace Retry
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class LogRecord : TableEntity
    {
        public int MessageId { get; set; }

        public int Attempt { get; set; }

        public string Result { get; set; }
    }
}
