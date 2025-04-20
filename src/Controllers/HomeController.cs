using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using eventphone.grafanalogo.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace eventphone.grafanalogo.Controllers
{
    [Produces("application/json")]
    public class HomeController:Controller
    {
        private static ConcurrentDictionary<string, Logo> _logos = new ConcurrentDictionary<string, Logo>();
        private readonly string _root;

        public HomeController(IWebHostEnvironment hostingEnvironment)
        {
            _root = Path.Combine(hostingEnvironment.WebRootPath, "images");
        }

        private Logo LoadImage(string filename)
        {
            var imagePath = Path.Combine(_root, filename);
            return LoadImageFromFile(imagePath);
        }

        private Logo UpdateImage(string filename, Logo logo)
        {
            var imagePath = Path.Combine(_root, filename);
            var file = new FileInfo(imagePath);
            if (logo.LastModified != file.LastWriteTime)
                logo = LoadImageFromFile(imagePath);
            return logo;
        }

        public static Logo LoadImageFromFile(string imagePath)
        {
            using (var image = Image.Load<Rgba32>(imagePath))
            {
                var fileInfo = new FileInfo(imagePath);
                var logo = new Logo(Path.GetFileNameWithoutExtension(imagePath), image.Width, fileInfo.LastWriteTime);
                NormalizeColors(image);
                for (int x = 0; x < image.Width; x++)
                {
                    var previousColor = image[x, image.Height - 1];
                    var previousValue = image.Height-1;
                    for (int y = image.Height - 1; y >= 0; y--)
                    {
                        var color = image[x, y];
                        if (!IsSimilar(previousColor, color))
                        {
                            logo.AddValue(x, previousValue - y, previousColor.ToHex());
                            previousColor = color;
                            previousValue = y;
                        }
                    }
                    logo.FinishColumn(x);
                }
                logo.Finish();
                return logo;
            }
        }

        private static void NormalizeColors(Image<Rgba32> image)
        {
            var colors = new List<Rgba32>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var color = image[x, y];
                    bool corrected = false;
                    foreach (var previousColor in colors)
                    {
                        if (IsSimilar(previousColor, color))
                        {
                            image[x, y] = previousColor;
                            corrected = true;
                            break;
                        }
                    }
                    if (!corrected)
                    {
                        colors.Add(color);
                    }
                }
            }
        }

        private static bool IsSimilar(Rgba32 left, Rgba32 right)
        {
            if (left == right) return true;
            if (left.Rgb.Equals(right.Rgb)) return true;
            if (Math.Abs(left.R - right.R) > 1) return false;
            if (Math.Abs(left.G - right.G) > 1) return false;
            if (Math.Abs(left.B - right.B) > 1) return false;
            return true;
        }

        public StatusCodeResult Index()
        {
            return StatusCode(200);
        }

        [HttpGet("metrics/find")]
        [HttpPost("metrics/find")]
        public IEnumerable<TreeJsonEntry> Find(string query)
        {
            var dir = new DirectoryInfo(_root);
            foreach (var file in dir.EnumerateFiles(query + "*"))
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                yield return new TreeJsonEntry{IsLeaf = true, Name = name, Path = name};
                name += "-scroll";
                yield return new TreeJsonEntry{IsLeaf = true, Name = name, Path = name};
            }
        }

        [HttpGet("render")]
        [HttpPost("render")]
        public IEnumerable<QueryResponse> Render(string[] target, string from, string until)
        {
            if (String.IsNullOrEmpty(from)) yield break;
            if (String.IsNullOrEmpty(until)) yield break;
            var start = DateParser.Parse(from).ToUnixTimeSeconds();
            var end = DateParser.Parse(until).ToUnixTimeSeconds();
            var range = end - start;
            var root = new DirectoryInfo(_root);
            foreach (var entry in target)
            {
                var scroll = false;
                var name = entry;
                if (entry.StartsWith("aliasSub(")){
                    //https://github.com/grafana/grafana/blob/f85e012e42fde1c173370b4c4ae43f8976557a90/pkg/tsdb/graphite/graphite.go#L230-L238
                    name = entry.Substring(9);
                    var index = name.IndexOf(',');
                    name = name.Substring(0, index);
                }
                if (name != null && name.EndsWith("-scroll"))
                {
                    name = name.Substring(0, name.Length - 7);
                    scroll = true;
                }
                var files = root.GetFiles(name + ".*");
                if (files.Length < 1)
                    continue;
                var file = Path.GetFileName(files[0].Name);
                var logo = _logos.AddOrUpdate(file, LoadImage, UpdateImage);
                foreach (var series in logo.Series)
                {
                    var step = range / logo.Datapoints;
                    if (scroll)
                    {
                        yield return new QueryResponse
                        {
                            Target = series.Name,
                            Datapoints = GetScrollDatapoints(series.Values, start, step, range)
                        };
                    }
                    else
                    {
                        yield return new QueryResponse
                        {
                            Target = series.Name,
                            Datapoints = series.Values.Select(x => ((double)x.Item2, (x.Item1 * step) + start))
                        };
                    }
                }
            }
        }

        [HttpGet("tags")]
        public IActionResult Tags()
        {
            return new JsonResult(Array.Empty<int>());
        }

        [HttpGet("tags/autoComplete/tags")]
        public IActionResult TagsAutoComplete()
        {
            return new JsonResult(Array.Empty<int>());
        }

        private static IEnumerable<(double, long)> GetScrollDatapoints(ICollection<(int, int)> values, long start, long step, long range)
        {
            step /= 2;
            var logoRange = range / 2;
            var delta = start % logoRange;
            var firstStart = (start) - delta;
            var lastValue = -1;
            foreach (var value in values)
            {
                var time = (value.Item1 * step) + firstStart;
                if (time < start)
                {
                    lastValue = value.Item2;
                    continue;
                }
                if (lastValue >= 0)
                {
                    yield return (lastValue, start);
                    lastValue = -1;
                }
                yield return (value.Item2, time);
            }
            var secondStart = (start - delta) + logoRange;
            foreach (var (time,value) in values)
            {
                yield return (value, (time * step) + secondStart);
            }
            var thirdStart = (start - delta) + range;
            var end = start + range;
            foreach (var (x,value) in values)
            {
                var time = (x * step) + thirdStart;
                if (time > end)
                {
                    yield return (value, end);
                    yield break;
                }
                yield return (value, time);
            }
        }
    }
}
