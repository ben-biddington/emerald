using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace Emerald.Cli
{
    // Add package:
    //
    //  dotnet add Emerald.Cli/Emerald.Cli.csproj package Selenium.WebDriver.ChromeDriver
    public class Program
    {
        static void Main(string[] args)
        {
            var log = new ConsoleLog();
            
            var options = Parse(args);

            log.Info($"Screenshotting <{options.Url.Count()}> urls and saving to <{options.Dir}>");

            log.Info($"{Environment.NewLine}--------------------------------------{Environment.NewLine}");

            var paddingSize = options.Url.Select(it => it.ToString().Length).Max();

            if (false == options.DryRun)
            {
                using var screen = new Screen(new Screen.Options { Headless = options.Headless });

                log.Info($"{Environment.NewLine}--------------------------------------{Environment.NewLine}");

                foreach (var url in options.Url)
                {
                    var path = screen.Shot(
                        log,
                        new Uri(url),
                        new DirectoryInfo(options.Dir));

                    log.Info($"{url.ToString().PadRight(paddingSize)} -> {path}");
                }
            }
        }

        private static Options Parse(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            if (result is NotParsed<Options> err)
                throw new Exception($"Failed to parse options with the following <{err.Errors.Count()}> errors:{Environment.NewLine}" +
                                    $"{string.Join(Environment.NewLine, err.Errors.Select(it => it))}");

            return ((Parsed<Options>)result).Value;
        }
    }

    public class ConsoleLog : Log
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    interface Log
    {
        void Info(string message);
    }

    public class Options
    {
        private string _dir = Path.GetTempPath();

        [Option('v', "verbose")]
        public bool Verbose { get; set; }

        [Option('b', "build")]
        public bool Build { get; set; }

        [Option('d', "dry")]
        public bool DryRun { get; set; }

        [Option('h', "headless")] 
        public bool Headless { get; set; } = false;

        [Option('d', "dir", HelpText = "Where to save the screenshots")]
        public string Dir
        {
            get => _dir;
            set => _dir = Path.GetFullPath(value);
        }

        [Value(0, MetaName = "url", HelpText = "The URL to screenshot")]
        public IEnumerable<string> Url { get; set; }
    }
}
