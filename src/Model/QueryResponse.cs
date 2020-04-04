using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace eventphone.grafanalogo.Model
{
    public class QueryResponse
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonIgnore]
        public IEnumerable<(double,long)> Datapoints { get; set; }

        [JsonPropertyName("datapoints")]
        public IEnumerable<object[]> JsonDatapoints
        {
            get
            {
                foreach (var (value, time) in Datapoints)
                {
                    yield return new object[] {value, time};
                }
            }
        }
    }
}