using System;
using System.IO;
using System.Linq;

using MELT;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace WikiExport.Test
{
    [TestFixture]
    [SetCulture("en-GB")] // The samples include culture-specific dates - force these tests to run with a culture that is compatible with the samples
    public class ExporterFixture
    {
        [TestCase("Samples/Sample.wiki/S1", "SS1", "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", null, "Sample S1")]
        [TestCase("Samples\\Sample.wiki\\S2", null, "Sample S1")]
        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        [TestCase("Samples\\Sample2", null, "Sample2")]
        public void DefaultOptions(string path, string file, string result)
        {
            var section = "Defaults";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section)
            };

            Export(section, result, options);
        }

        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        public void NoAppendix(string path, string file, string result)
        {
            var section = "NoAppendix";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section),
                AppendixProcessing = false
            };

            Export(section, result, options);
        }

        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "Sample SS1")]
        public void Caption(string path, string file, string result)
        {
            var section = "Caption";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section),
                RetainCaption = true
            };

            Export(section, result, options);
        }

        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        public void ValidationError(string path, string file, string result)
        {
            var section = "Errors";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = TestPath(path),
                RetainCaption = true
            };

            // NOTE: We test the actual validation errors elsewhere
            Assert.Throws<Exception>(() => Export(section, result, options));
        }

        [TestCase("Samples\\Missing.wiki", null, "Missing")]
        public void Missing(string path, string file, string result)
        {
            var section = "Missing";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section),
                RetainCaption = true,
                FatalErrorLevel = LogLevel.Critical
            };

            Export(section, result, options);
        }

        [TestCase("Samples\\Missing.wiki", null, "Missing")]
        public void MissingException(string path, string file, string result)
        {
            var section = "Missing";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section),
                RetainCaption = true
            };

            var loggerFactory = TestLoggerFactory.Create();
            var logger = loggerFactory.CreateLogger<Exporter>();

            Assert.Throws<Exception>(() => Export(section, result, options, logger));

            var logs = loggerFactory.Sink.LogEntries.ToList();
            var missingOrder = logs.Where(x => x.Message != null && x.Message.StartsWith("No .order file in")).ToList();
            Assert.That(missingOrder.Count(), Is.EqualTo(1));

            var missingFile = logs.Where(x => x.Message != null && x.Message.StartsWith("No S2.md in")).ToList();
            Assert.That(missingFile.Count(), Is.EqualTo(1));
        }

        private void Export(string section, string result, ExportOptions options, ILogger<Exporter> logger = null)
        {
            logger ??= new NullLogger<Exporter>();

            var exporter = new Exporter(logger)
            {
                ExportDate = new DateTime(2021, 9, 1)
            };

            exporter.Export(options);

            CompareFile(section, result);
        }

        private void CompareFile(string path, string name)
        {
            var sourceFile = OutputPath(path).WikiFileName(name);
            var targetFile = ResultPath(path).WikiFileName(name);

            var sourceData = File.Exists(sourceFile) ? File.ReadAllText(sourceFile) : null;
            var targetData = File.Exists(targetFile) ? File.ReadAllText(targetFile) : null;

            Assert.That(sourceData, Is.EqualTo(targetData));
        }

        private string TestPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }

        private string OutputPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Output", path);
        }

        private string ResultPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Results", path);
        }
    }
}
