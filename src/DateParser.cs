using System;
using System.Text.RegularExpressions;

namespace eventphone.grafanalogo
{
    public class DateParser
    {
        private static readonly Regex[] AbsoluteFormats = new[]
        {
            new Regex(@"^(?<H>\d\d):(?<M>\d\d)_(?<y>\d\d)(?<m>\d\d)(?<d>\d\d)$", RegexOptions.Compiled),
            new Regex(@"^(?<y>\d\d\d\d)(?<m>\d\d)(?<d>\d\d)$", RegexOptions.Compiled),
            new Regex(@"^(?<m>\d\d)/(?<d>\d\d)/(?<y>\d\d)$", RegexOptions.Compiled),
            new Regex(@"^(?<H>\d\d):(?<M>\d\d)_(?<y>\d\d\d\d)(?<m>\d\d)(?<d>\d\d)$", RegexOptions.Compiled),
        };

        private static readonly Regex EpochFormat = new Regex(@"^(?<t>\d{8,})$", RegexOptions.Compiled);

        public static DateTimeOffset Parse(string offset)
        {
            if (String.IsNullOrEmpty(offset))
                throw new ArgumentNullException(nameof(offset));
            if (offset[0] == '-' || offset[0] == '+')
            {
                //relative Time
                return DateTimeOffset.UtcNow + ParseRelative(offset);
            }
            return ParseAbsolute(offset);
        }

        public static TimeSpan ParseRelative(string offset)
        {
            var last = offset[offset.Length - 1];
            int suffixLength;
            Func<long, TimeSpan> parser;
            switch (last)
            {
                case 's':
                    suffixLength = 1;
                    parser = x => TimeSpan.FromSeconds(-x);
                    break;
                case 'h':
                    suffixLength = 1;
                    parser = x => TimeSpan.FromHours(-x);
                    break;
                case 'd':
                    suffixLength = 1;
                    parser = x => TimeSpan.FromDays(-x);
                    break;
                case 'w':
                    suffixLength = 1;
                    parser = x => TimeSpan.FromDays(-x * 7);
                    break;
                case 'y':
                    suffixLength = 1;
                    parser = x => DateTimeOffset.UtcNow.AddYears((int)-x) - DateTimeOffset.UtcNow;
                    break;
                case 'n':
                    suffixLength = 3;
                    if (offset[offset.Length - 3] == 'm')
                    {
                        if (offset[offset.Length - 2] == 'i')
                        {
                            parser = x => TimeSpan.FromMinutes(-x);
                            break;
                        }
                        else if (offset[offset.Length - 2] == 'o')
                        {
                            parser = x => DateTimeOffset.UtcNow.AddMonths((int)-x) - DateTimeOffset.UtcNow;
                            break;
                        }
                    }
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            var value = offset.Substring(1, offset.Length - suffixLength - 1);
            var numeric = Int64.Parse(value);
            if (offset[0] == '+')
                numeric *= -1;
            return parser(numeric);
        }

        private static DateTimeOffset ParseAbsolute(string offset)
        {
            if (offset.Length > 8)
            {
                var m = EpochFormat.Match(offset);
                if (m.Success)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(m.Groups["t"].Value));
                }
            }
            if (offset == "now")
                return DateTimeOffset.UtcNow;
            foreach (var regex in AbsoluteFormats)
            {
                var m = regex.Match(offset);
                if (m.Success)
                {
                    var hour = m.Groups["H"].Value;
                    var min = m.Groups["M"].Value;
                    var year = m.Groups["y"].Value;
                    if (year.Length == 2)
                        year = "20" + year;
                    var month = m.Groups["m"].Value;
                    var day = m.Groups["d"].Value;
                    if (min == String.Empty) min = "0";
                    if (hour == String.Empty) hour = "0";
                    return new DateTimeOffset(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day),
                        Int32.Parse(hour), Int32.Parse(min), 0, TimeSpan.Zero);
                }
            }
            throw new ArgumentException("invalid time period");
        }
    }
}
