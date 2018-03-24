namespace Common.Chaining
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class OrderRecord : TableEntity
    {
        public string CustomerName { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
