using System;
using System.Collections.Generic;
using System.Linq;

namespace eventphone.grafanalogo.Model
{
    public class LogoSeries
    {
        private readonly Dictionary<int,int> _datapoints;

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

        public void CleanDuplicates()
        {
            if (_datapoints.Count >= 3)
            {
                using (var enumerator = _datapoints.ToList().GetEnumerator())
                {
                    enumerator.MoveNext();
                    var first = enumerator.Current;
                    enumerator.MoveNext();
                    var second = enumerator.Current;
                    while (first.Value == 0 && second.Value == 0 && enumerator.MoveNext())
                    {
                        _datapoints.Remove(first.Key);
                        first = second;
                        second = enumerator.Current;
                    }
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (first.Value == second.Value && second.Value == current.Value)
                        {
                            _datapoints.Remove(second.Key);
                        }
                        else
                        {
                            first = second;
                        }
                        second = current;
                    }
                }
            }
            _values = CleanValues(CalculateValues()).ToArray();
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

        public bool IsEmpty => _datapoints.Count == 0;

        private static IEnumerable<(int, int)> CleanValues(IEnumerable<(int, int)> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext()) yield break;
                var first = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    yield return first;
                    yield break;
                }
                var second = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (first.Item2 != second.Item2 || first.Item2 != current.Item2)
                    {
                        yield return first;
                        first = second;
                    }
                    second = current;
                }
                yield return first;
                yield return second;
            }
        }
    }
}