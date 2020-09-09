using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Emerald.Cli
{
    internal class Screen : IDisposable
    {
        private readonly Log _log;
        private readonly IWebDriver _driver;

        internal class Options
        {
            public bool Headless { get; set; }
            public string Browser { get; set; }
        }

        public Screen(Log log, Options opts)
        {
            _log = log;
            _driver = opts.Browser == "chrome" ? Drivers.Chrome(opts) : Drivers.Firefox(opts);
        }

        public string Shot(Uri url, DirectoryInfo targetDirectory)
        {
            Ensure(targetDirectory);

            return Take(url, targetDirectory, $"{Safe(url.ToString())}-{DateTime.Now.Ticks}.png");
        }

        private string Take(Uri url, DirectoryInfo targetDirectory, string defaultFilename)
        {

            _driver.Manage().Window.Maximize();

            _driver.Navigate().GoToUrl(url);

            _driver.ExecuteJavaScript($"window.scrollTo(0,0)");

            var pageHeight = Convert.ToInt32(_driver.ExecuteJavaScript<long>("return Math.max(document.body.scrollHeight, document.body.offsetHeight);"));
            var viewPortHeight = Convert.ToInt32(_driver.ExecuteJavaScript<long>("return window.innerHeight;"));
            var viewPortWidth = Convert.ToInt32(_driver.ExecuteJavaScript<long>("return window.innerWidth;"));

            var filename = $"{Safe(_driver.Title.ToLower())}-{DateTime.Now.Ticks}.png";

            using (var finalImage = new Bitmap(Convert.ToInt32(viewPortWidth), Convert.ToInt32(pageHeight)))
            using (var g = Graphics.FromImage(finalImage))
            {
                var snap = new Snap(_driver, g);

                var location = new Point(0, 0);

                snap.Take(location);

                while (_driver.ExecuteJavaScript<bool>("return (window.innerHeight + window.scrollY) < document.body.scrollHeight;"))
                {
                    var scrollingRemaining = Convert.ToInt32(_driver.ExecuteJavaScript<long>("return document.body.scrollHeight - (window.innerHeight + window.scrollY)"));

                    var howMuchToScrollBy = Math.Min(scrollingRemaining, viewPortHeight);

                    _driver.ExecuteJavaScript($"window.scrollBy(0,{howMuchToScrollBy})");

                    location.Offset(0, howMuchToScrollBy);

                    snap.Take(location);
                }

                g.Flush();

                var cropped = ShaveChrome(finalImage);

                var path = Path.Combine(targetDirectory.FullName, filename);
                
                cropped.Save(path, ImageFormat.Png);
                
                return path;
            }
        }

        private string Safe(string text)
        {
            var result = text;

            foreach (var invalidFilenameCharacter in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalidFilenameCharacter, '-');
            }

            return result.Replace(':', '-');
        }

        // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/cropping-and-scaling-images-in-gdi?view=netframeworkdesktop-4.8
        private Bitmap ShaveChrome(Bitmap source)
        {
            // https://davidwalsh.name/detect-scrollbar-width
            var scrollBarWidth = Convert.ToInt32(_driver.ExecuteJavaScript<long>(
                @"return function() {
                    var scrollDiv = document.createElement('div');
                    
                    scrollDiv.setAttribute('style', 'width: 100px;height: 100px;overflow: scroll;position: absolute;top: -9999px;');
                    
                    document.body.appendChild(scrollDiv);

                    var scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;

                    document.body.removeChild(scrollDiv);

                    return scrollbarWidth;
                }()"));

            var cropRectangle = new Rectangle(0, 0, source.Width - scrollBarWidth, source.Height);

            var cropped = new Bitmap(cropRectangle.Width, cropRectangle.Height);

            using (var g = Graphics.FromImage(cropped))
            {
                g.DrawImage(source, new Rectangle(0, 0, cropped.Width, cropped.Height), cropRectangle, GraphicsUnit.Pixel);
            }

            return cropped;
        }

        private void Ensure(DirectoryInfo targetDirectory)
        {
            if (false == targetDirectory.Exists)
            {
                targetDirectory.Create();
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
    }

    internal class Snap
    {
        private readonly IWebDriver _driver;
        private readonly Graphics _graphics;

        internal Snap(IWebDriver driver, Graphics graphics)
        {
            _driver = driver;
            _graphics = graphics;
        }

        internal void Take(Point location)
        {
            using var m = new MemoryStream(_driver.TakeScreenshot().AsByteArray);

            _graphics.DrawImage(Image.FromStream(m), location);
        }
    }
}