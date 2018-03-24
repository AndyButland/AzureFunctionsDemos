namespace Common.Chaining
{
    using Newtonsoft.Json;

    public class OrderDetail
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerEmail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
