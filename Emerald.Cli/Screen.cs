﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Emerald.Cli
{
    internal class Screen : IDisposable
    {
        private readonly IWebDriver _driver;

        internal class Options
        {
            public bool Headless { get; set; }
        }

        public Screen(Options opts)
        {
            _driver = Drivers.Chrome(opts);
        }

        public string Shot(Log log, Uri url, DirectoryInfo targetDirectory)
        {
            Ensure(targetDirectory);

            return Take(_driver, url, targetDirectory, $"{Safe(url.ToString())}-{DateTime.Now.Ticks}.png");
        }

        private string Safe(string text)
        {
            var result = text;

            foreach (var invalidFilenameCharacter in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalidFilenameCharacter, '-');
            }

            return result;
        }

        private string Take(IWebDriver driver, Uri url, DirectoryInfo targetDirectory, string fileName)
        {
            var path = Path.Combine(targetDirectory.FullName, fileName);

            driver.Manage().Window.Maximize();

            driver.Navigate().GoToUrl(url);

            driver.ExecuteJavaScript($"window.scrollTo(0,0)");

            var pageHeight = Convert.ToInt32(driver.ExecuteJavaScript<long>("return Math.max(document.body.scrollHeight, document.body.offsetHeight);"));
            var viewPortHeight = Convert.ToInt32(driver.ExecuteJavaScript<long>("return window.innerHeight;"));
            var viewPortWidth = Convert.ToInt32(driver.ExecuteJavaScript<long>("return window.innerWidth;"));

            using (var finalImage = new Bitmap(Convert.ToInt32(viewPortWidth), Convert.ToInt32(pageHeight)))
            using (var g = Graphics.FromImage(finalImage))
            {
                var snap = new Snap(driver, g);

                var location = new Point(0, 0);

                snap.Take(location);

                while (driver.ExecuteJavaScript<bool>("return (window.innerHeight + window.scrollY) < document.body.scrollHeight;"))
                {
                    var scrollingRemaining = Convert.ToInt32(driver.ExecuteJavaScript<long>("return document.body.scrollHeight - (window.innerHeight + window.scrollY)"));

                    var howMuchToScrollBy = Math.Min(scrollingRemaining, viewPortHeight);

                    driver.ExecuteJavaScript($"window.scrollBy(0,{howMuchToScrollBy})");

                    location.Offset(0, howMuchToScrollBy);

                    snap.Take(location);
                }

                g.Flush();

                finalImage.Save(path, ImageFormat.Png);
            }

            return path;
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