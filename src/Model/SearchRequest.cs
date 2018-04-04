using Newtonsoft.Json;

namespace eventphone.grafanalogo.Model
{
    public class SearchRequest
    {
        [JsonProperty("target")]
        public string Target { get; set; }
    }
}