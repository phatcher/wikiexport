using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace WikiExport.Test
{
    [TestFixture]
    public class AttachmentFixerFixture
    {
        [Test]
        public void Root()
        {
            Fix("Root", true);
        }

        [Test]
        public void NonRoot()
        {
            Fix("NonRoot", true, "SubDir");
        }

        [Test]
        public void Subdirectory()
        {
            Fix("Subdirectory", false);
        }

        [Test]
        public void NoCaption()
        {
            Fix("NoCaption", true);
        }

        [Test]
        public void MissingImage()
        {
            Fix("MissingImage", true);
        }

        public void Fix(string name, bool retainCaption, string path = "")
        {
            var source = File.ReadAllText(SamplesPath(Path.Combine(path, $"{name}.md")));
            var sourceAttachmentsPath = SamplesPath(".attachments");

            // Standard name for the attachments output
            var attachmentPath = Path.Combine(name, "Attachments");
            var outputAttachmentsPath = OutputPath(attachmentPath);

            // Split the results by the name of the source
            var resultsPath = ResultPath(name);
            var resultsAttachmentPath = Path.Combine(resultsPath, "Attachments");

            var expected = File.ReadAllText(Path.Combine(resultsPath, path, $"{name}.md"));

            var fixer = new AttachmentFixer(sourceAttachmentsPath, outputAttachmentsPath, retainCaption);

            // Act
            var candidate = fixer.Fix(source);

            // Assert
            Assert.That(candidate, Is.EqualTo(expected));

            var outputFiles = AttachmentFiles(outputAttachmentsPath);
            var resultsFiles = AttachmentFiles(resultsAttachmentPath);

            Assert.That(outputFiles.Count, Is.EqualTo(resultsFiles.Count), "Attachment count differs");

            Assert.That(outputFiles, Is.EquivalentTo(resultsFiles), "Attachments differ");
        }

        private IList<string> AttachmentFiles(string path)
        {
            var results = new List<string>();

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
                results.AddRange(files.Select(x => x.Remove(0, path.Length)));
            }

            return results;
        }

        private string TestPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }

        /// <summary>
        /// Combine the test samples directory with an additional path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string SamplesPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Attachments", path);
        }

        /// <summary>
        /// Combine the test output directory with an additional path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string OutputPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Output", "Attachments", path);
        }

        /// <summary>
        /// Combine the test results directory with an additional path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ResultPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "Results", "Attachments", path);
        }
    }
}