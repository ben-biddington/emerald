using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Emerald.Cli
{
    internal static class Drivers
    {
        internal static IWebDriver Chrome(Screen.Options opts)
        {
            var options = new ChromeOptions().Tap(it =>
            {
                it.AddArguments("--disable-extensions", "window-size=1920,1080");
                
                if (opts.Headless)
                {
                    it.AddArgument("--headless");
                }
                it.AddArgument("--start-maximized");
                it.AddArgument("--no-sandbox");
                it.AddArgument("--disable-popup-blocking");
                it.AddArgument("--disable-extensions");
                it.AddArgument("window-size=1920,1080");

                it.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                it.SetLoggingPreference(LogType.Client, LogLevel.Off);
                it.SetLoggingPreference(LogType.Server, LogLevel.Off);
                it.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                it.SetLoggingPreference(LogType.Profiler, LogLevel.Off);

                it.AddAdditionalCapability(CapabilityType.HasNativeEvents, false, true);
                it.AddAdditionalCapability("idleTimeout", TimeSpan.FromSeconds(30), true);
            });

            return new ChromeDriver(options).Tap(driver =>
            {
                driver.FileDetector = new LocalFileDetector();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);
                driver.Manage().Window.Maximize();
            });
        }
    }
}