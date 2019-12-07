using System;
using System.Collections.Generic;

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
                            //move an exit
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
            for (int i = Series.Count - 1; i >= 0; i--)
            {
                var series = Series[i];
                if (series.ContainsKey(x))
                    return;
                series.AddValue(x, 0);
            }
        }

        internal void Finish()
        {
            foreach (var series in Series)
            {
                series.CleanDuplicates();
            }
        }
    }
}