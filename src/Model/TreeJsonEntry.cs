using System.Text.Json.Serialization;

namespace eventphone.grafanalogo.Model
{
    public class TreeJsonEntry
    {
        [JsonPropertyName("leaf")]
        public int Leaf { get { return IsLeaf ? 1 : 0; } }

        [JsonPropertyName("allowChildren")]
        public int AllowChildren { get { return IsLeaf ? 0 : 1; } }

        [JsonPropertyName("expandable")]
        public int Expandable { get { return IsLeaf ? 0 : 1; } }

        [JsonIgnore]
        public bool IsLeaf { get; set; }

        [JsonPropertyName("text")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Path { get; set; }

        [JsonPropertyName("context")]
        public object Context { get { return new object(); } }
    }
}
