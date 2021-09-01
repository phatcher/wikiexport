using System;
using System.IO;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

namespace WikiExport.Test
{
    [TestFixture]
    public class ExporterFixture
    {
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
