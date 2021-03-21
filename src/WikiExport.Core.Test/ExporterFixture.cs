using System.IO;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

namespace WikiExport.Test
{
    [TestFixture]
    public class ExporterFixture
    {
        [TestCase("Samples\\Sample.wiki\\S1", "SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", null)]
        [TestCase("Samples\\Sample.wiki\\S2", null)]
        [TestCase("Samples\\Sample.wiki", null)]
        [TestCase("Samples\\Sample2", null)]
        public void DefaultOptions(string path, string file)
        {
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
                TargetPath = OutputPath("Defaults")
            };

            var logger = new Mock<ILogger<Exporter>>();

            var exporter = new Exporter(logger.Object);

            exporter.Export(options);
        }

        private string TestPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }


        private string OutputPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Output", path);
        }
    }
}
