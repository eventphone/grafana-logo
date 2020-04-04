using System.Text.Json.Serialization;

namespace eventphone.grafanalogo.Model
{
    public class SearchRequest
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }
    }
}