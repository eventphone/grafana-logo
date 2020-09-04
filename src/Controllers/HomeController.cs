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

        public HomeController(IHostingEnvironment hostingEnvironment)
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
            return Search(new SearchRequest { Target = query })
                .Select(x => new TreeJsonEntry { IsLeaf = true, Name = x.Name, Path = x.Key });
        }
        
        [HttpPost]
        public IEnumerable<Variable> Search([FromBody]SearchRequest request)
        {
            var target = request?.Target ?? String.Empty;
            var dir = new DirectoryInfo(_root);
            foreach (var file in dir.EnumerateFiles(target + "*"))
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                yield return new Variable(name, name);
                name += "-scroll";
                yield return new Variable(name, name);
            }
        }

        [HttpGet("render")]
        [HttpPost("render")]
        public IEnumerable<QueryResponse> Render(string[] target, string from, string until)
        {
            var fromdate = DateParser.Parse(from);
            var todate = DateParser.Parse(until);
            return Query(new QueryRequest
            {
                Targets = target.Select(x => new QueryTarget { Target = x, Type = "timeserie" }).ToArray(),
                Range = new QueryRange { From = fromdate, To = todate }
            }).Select(x => new QueryResponse
            {
                Target = x.Target,
                Datapoints = x.Datapoints.Select(d => (d.Item1, d.Item2 / 1000))
            });
        }

        [HttpPost]
        public IEnumerable<QueryResponse> Query([FromBody] QueryRequest request)
        {
            var start = request.Range.From.ToUnixTimeMilliseconds();
            var until = request.Range.To.ToUnixTimeMilliseconds();
            var range = until - start;
            var root = new DirectoryInfo(_root);
            foreach (var target in request.Targets.Where(x => x.Type == "timeserie"))
            {
                var scroll = false;
                var name = target.Target;
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
