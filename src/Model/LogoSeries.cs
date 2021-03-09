using System;
using System.Collections.Generic;
using System.Linq;

namespace eventphone.grafanalogo.Model
{
    public class LogoSeries
    {
        private Dictionary<int,int> _datapoints;

        public LogoSeries()
        {
            _datapoints = new Dictionary<int, int>();
        }

        public string Name { get; set; }

        private IList<(int, int)> _values = new (int,int)[0];
        public ICollection<(int, int)> Values
        {
            get
            {
                return _values;
            }
        }

        public bool AddValue(int x, int y)
        {
            if (_datapoints.Count == 0)
            {
                _datapoints.Add(x, y);
                return true;
            }
            else if (!_datapoints.TryGetValue(x, out var existing))
            {
                _datapoints.Add(x, y);
                return true;
            }
            else if (existing == y)
            {
                return true;
            }
            return false;
        }

        public bool ContainsKey(int x)
        {
            return _datapoints.ContainsKey(x);
        }

        public int GetValue(int x)
        {
            if (_datapoints.TryGetValue(x, out var value))
                return value;
            return 0;
        }

        public void Calculate()
        {
            _values = CalculateValues().ToArray();
            _datapoints = new Dictionary<int, int>();
        }

        private IEnumerable<(int, int)> CalculateValues()
        {
            var previous = 0;
            var previousKey = -1;
            foreach (var x in _datapoints.OrderBy(d=>d.Key))
            {
                if ((x.Key - previousKey) == 1 || previous == 0)
                    yield return (x.Key, previous);
                yield return (x.Key, x.Value);
                previous = x.Value;
                previousKey = x.Key;
            }
        }

        internal IDictionary<int, int> Datapoints => _datapoints;
    }
}