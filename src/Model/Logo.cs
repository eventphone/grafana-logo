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
            if (y == 1)
            {

            }
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
            var newSeries = new LogoSeries { Name = color };
            newSeries.AddValue(x, y);
            Series.Add(newSeries);
        }

        internal void FinishColumn(int x, int y)
        {
            for (int i = Series.Count - 1; i >= 0; i--)
            {
                var series = Series[i];
                if (series.ContainsKey(x))
                    return;
                series.AddValue(x, y);
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