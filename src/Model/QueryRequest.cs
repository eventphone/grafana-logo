namespace eventphone.grafanalogo.Model
{
    public class QueryRequest
    {
        public QueryRange Range { get; set; }

        public QueryTarget[] Targets { get; set; }

        public ulong MaxDataPoints { get; set; }
    }
}