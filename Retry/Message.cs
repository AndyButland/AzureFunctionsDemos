namespace Retry
{
    using Newtonsoft.Json;

    public class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
