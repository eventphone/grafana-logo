using Newtonsoft.Json;

namespace eventphone.grafanalogo.Model
{
    public class TreeJsonEntry
    {
        [JsonProperty("leaf")]
        public int Leaf { get { return IsLeaf ? 1 : 0; } }

        [JsonProperty("allowChildren")]
        public int AllowChildren { get { return IsLeaf ? 0 : 1; } }

        [JsonProperty("expandable")]
        public int Expandable { get { return IsLeaf ? 0 : 1; } }

        [JsonIgnore]
        public bool IsLeaf { get; set; }

        [JsonProperty("text")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Path { get; set; }

        [JsonProperty("context")]
        public object Context { get { return new object(); } }
    }
}
