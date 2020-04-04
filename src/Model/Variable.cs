using System.Text.Json.Serialization;

namespace eventphone.grafanalogo.Model
{
    public class Variable
    {
        public Variable(string name, string key)
        {
            Name = name;
            Key = key;
        }

        public Variable()
        {
        }

        [JsonPropertyName("value")]
        public string Key { get; set; }

        [JsonPropertyName("text")]
        public string Name { get; set; }
    }
}
