using System;
using System.IO;

using Microsoft.Extensions.Logging;

using Moq;

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
            var ex = Assert.Throws<Exception>(() => Export(section, result, options));
        }

        [TestCase("Samples\\MissingOrder.wiki", null, "Sample")]
        public void MissingOrder(string path, string file, string result)
        {
            var section = "MissingOrder";
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath(section),
                RetainCaption = true
            };

            var ex = Assert.Throws<Exception>(() => Export(section, result, options));
            Assert.That(ex.Message, Does.StartWith("No .order file in"));
        }

        private void Export(string section, string result, ExportOptions options)
        {
            var logger = new Mock<ILogger<Exporter>>();

            var exporter = new Exporter(logger.Object)
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

            var sourceData = File.ReadAllText(sourceFile);
            var targetData = File.ReadAllText(targetFile);

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
