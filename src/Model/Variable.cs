using Newtonsoft.Json;

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

        [JsonProperty("value")]
        public string Key { get; set; }

        [JsonProperty("text")]
        public string Name { get; set; }
    }
}
