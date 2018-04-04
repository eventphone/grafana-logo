using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace eventphone.grafanalogo.Model
{
    public class QueryResponse
    {
        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonIgnore]
        public IEnumerable<Tuple<double,long>> Datapoints { get; set; }

        [JsonProperty("datapoints")]
        public IEnumerable<object[]> JsonDatapoints
        {
            get
            {
                foreach (var datapoint in Datapoints)
                {
                    yield return new object[] {datapoint.Item1, datapoint.Item2};
                }
            }
        }
    }
}