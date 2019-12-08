using System;
using System.Collections.Generic;
using System.Linq;

namespace eventphone.grafanalogo.Model
{
    public class Logo
    {
        public Logo(string name, int datapoints, DateTime lastModified)
        {
            Name = name;
            Datapoints = datapoints;
            LastModified = lastModified;
            Series = new List<LogoSeries>();
        }

        public DateTime LastModified { get; }

        public string Name { get; }

        public List<LogoSeries> Series { get; }

        public int Datapoints { get; }

        internal void AddValue(int x, int y, string color)
        {
            var skip = -1;
            for (int i = 0; i < Series.Count; i++)
            {
                if (Series[i].ContainsKey(x))
                    skip = i;
            }
            for (int i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                if (i <= skip)
                {
                    if (!series.ContainsKey(x))
                        series.AddValue(x, 0);
                    continue;
                }
                if (series.Name != color)
                {
                    series.AddValue(x, 0);
                    continue;
                }
                if (!series.AddValue(x, y))
                {
                    continue;
                }
                return;
            }
            //can we move up an existing color?
            for (int i = 0; i < Series.Count; i++)
            {
                var existing = Series[i];
                if (existing.Name == color)
                {
                    if (!existing.ContainsKey(x) || existing.Datapoints[x] == 0)
                    {
                        //found color without current value - now check if all previous values until skip are zero
                        bool canbeMoved = true;
                        for (int j = i+1; j <= skip; j++)
                        {
                            foreach (var previousKey in existing.Datapoints.Keys)
                            {
                                if (previousKey == x) continue;
                                if (Series[j].ContainsKey(previousKey) && Series[j].Datapoints[previousKey] != 0)
                                {
                                    canbeMoved = false;
                                    break;
                                }
                            }
                            if (!canbeMoved)
                                break;
                        }
                        if (canbeMoved)
                        {
                            //move and exit
                            Series.Remove(existing);
                            Series.Insert(skip, existing);
                            existing.Datapoints[x] = y;
                            for (int j = Series.Count - 1; j > skip + 1; j--)
                            {
                                if (Series[j].Datapoints[x] == 0)
                                    Series[j].Datapoints.Remove(x, out _);
                                else
                                    break;
                            }
                            return;
                        }
                    }
                }
            }
            var newSeries = new LogoSeries { Name = color };
            newSeries.AddValue(x, y);
            Series.Insert(skip + 1, newSeries);
            for (int i = Series.Count - 1; i > skip + 1; i--)
            {
                if (Series[i].Datapoints[x] == 0)
                    Series[i].Datapoints.Remove(x, out _);
                else
                    break;
            }
        }

        internal void FinishColumn(int x)
        {
            if (x > 0)
                AdjustNeighbors(x-1, x);
            for (int i = Series.Count - 1; i >= 0; i--)
            {
                var series = Series[i];
                if (series.ContainsKey(x))
                    return;
                series.AddValue(x, 0);
            }
        }

        private void AdjustNeighbors(int previous, int current)
        {
            var left = new Stack<int>();
            var right = new Stack<int>();
            foreach (var series in Series)
            {
                left.Push(series.GetValue(previous));
                right.Push(series.GetValue(current));
            }
            for (int i = Series.Count - 1; i >= 0; i--)
            {
                var series = Series[i];
                var lValue = left.Pop();
                var rValue = right.Pop();
                if (left.Count != right.Count)
                    throw new InvalidOperationException();
                if (lValue > 0 && rValue == 0)
                {
                    var lsum = left.Sum();
                    var rsum = right.Sum();
                    if (lsum <= rsum)
                    {
                        int j;
                        bool swapped = false;
                        for (j = i-1; j >= 0; j--)
                        {
                            var lowerSeries = Series[j];
                            var value = right.Pop();
                            if (lowerSeries.Name == series.Name)
                            {
                                if (value == 0)
                                {
                                    continue;
                                }
                                if ((lsum + lValue) < right.Sum())
                                {
                                    right.Push(value);
                                    break;
                                }
                                series.Datapoints[current] = value;
                                right.Push(0);
                                lowerSeries.Datapoints[current] = 0;
                                swapped = true;
                                break;
                            }
                            if (value != 0)
                            {
                                right.Push(value);
                                break;
                            }
                        }
                        for (j++; j < i; j++)
                        {
                            right.Push(0);
                            if (swapped)
                                Series[j].AddValue(current, 0);
                        }
                    }
                }
            }
        }

        internal void Finish()
        {
            var keys = Series[0].Datapoints.Keys.Reverse().ToList();
            var previous = keys[0];
            for (int i = 1; i < keys.Count; i++)
            {
                var current = keys[i];
                AdjustNeighbors(previous, current);
                previous = current;
            }
            foreach (var series in Series)
            {
                series.CleanDuplicates();
            }
        }
    }
}