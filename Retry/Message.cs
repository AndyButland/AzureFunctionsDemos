namespace Retry
{
    using Newtonsoft.Json;

    public class Message
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
