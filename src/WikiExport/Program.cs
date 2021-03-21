using System;

using CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WikiExport
{
    class Program
    {
        static void Main(string[] args)
        {
            // Based on https://wildermuth.com/2020/08/02/NET-Core-Console-Apps---A-Better-Way
            Parser.Default.ParseArguments<ExportOptions>(args)
                  .WithParsed(options =>
                  {
                      Console.WriteLine("wikiexport");
                      Console.WriteLine($"Exporting {options.SourcePath} {options.SourceFile} to {options.TargetPath}");
                      Console.WriteLine();

                      var host = Host.CreateDefaultBuilder()
                          .ConfigureServices((b, c) =>
                          {
                              c.AddSingleton(options);
                          })
                          .ConfigureLogging(bldr =>
                          {
                              bldr.ClearProviders();
                              bldr.AddConsole()
                                  .SetMinimumLevel(LogLevel.Error);
                          })
                          .Build();

                      var exporter = ActivatorUtilities.CreateInstance<Exporter>(host.Services);
                      exporter.Export(options);
                  });
        }
    }
}