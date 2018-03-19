namespace Common.Sharding
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class ResultRecord : TableEntity
    {
        public string Results { get; set; }
    }
}
