using System.IO;

using NUnit.Framework;

namespace WikiExport.Test
{
    [TestFixture]
    public class ExportOptionsExtensionsFixture
    {
        [Test]
        public void ProjectNameSpecified()
        {
            var expected = "Foo";

            var options = new ExportOptions
            {
                Project = expected
            };

            var candidate = options.ProjectName();

            Assert.That(candidate, Is.EqualTo(expected), "Project name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", "Sample")]
        [TestCase("Samples\\Sample.wiki\\S2", "Sample")]
        [TestCase("Samples\\Sample.wiki", "Sample")]
        [TestCase("Samples\\Sample2", "Sample2")]
        [TestCase("Samples", null)]
        public void ProjectNameFromWikiDirectory(string path, string expected)
        {
            var options = new ExportOptions
            {
                SourcePath = TestPath(path)
            };

            var candidate = options.ProjectName();

            Assert.That(candidate, Is.EqualTo(expected), "Project name differs");
        }

        [Test]
        public void DocumentTitleSpecified()
        {
            var expected = "Foo";

            var options = new ExportOptions
            {
                Title = expected
            };

            var candidate = options.DocumentTitle();

            Assert.That(candidate, Is.EqualTo(expected), "DocumentTitle name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", null, "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "My-Section", "Sample My Section")]
        [TestCase("Samples\\Sample.wiki\\S2", null, "Sample S2")]
        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        [TestCase("Samples\\Sample2", null, "Sample2")]
        [TestCase("Samples", null, "Samples")]
        public void DocumentTitleIncludeProject(string path, string file, string expected)
        {
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file
            };

            var candidate = options.DocumentTitle();

            Assert.That(candidate, Is.EqualTo(expected), "DocumentTitle name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", null, "SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "My-Section", "My Section")]
        [TestCase("Samples\\Sample.wiki\\S2", null, "S2")]
        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        [TestCase("Samples\\Sample2", null, "Sample2")]
        [TestCase("Samples", null, "Samples")]
        public void DocumentTitleExcludeProject(string path, string file, string expected)
        {
            var options = new ExportOptions
            {
                SourcePath     = TestPath(path),
                SourceFile     = file,
                ProjectInTitle = false
            };

            var candidate = options.DocumentTitle();

            Assert.That(candidate, Is.EqualTo(expected), "DocumentTitle name differs");
        }

        [Test]
        public void TargetFileSpecfied()
        {
            var expected = "Foo";

            var options = new ExportOptions
            {
                TargetFile = expected
            };

            var candidate = options.SelectTargetFile();

            Assert.That(candidate, Is.EqualTo(expected), "Target name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", null, "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "Sample SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "My-Section", "Sample My Section")]
        [TestCase("Samples\\Sample.wiki\\S2", null, "Sample S2")]
        [TestCase("Samples\\Sample.wiki", null, "Sample")]
        [TestCase("Samples\\Sample2", null, "Sample2")]
        [TestCase("Samples", null, "Samples")]
        public void TargetFileDerived(string path, string file, string expected)
        {
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file,
            };

            var candidate = options.SelectTargetFile();

            Assert.That(candidate, Is.EqualTo(expected), "Target name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", "Sample.wiki")]
        [TestCase("Samples\\Sample.wiki\\S2", "Sample.wiki")]
        [TestCase("Samples\\Sample.wiki", "Sample.wiki")]
        [TestCase("Samples\\Sample2", "Sample2")]
        public void WikiRoot(string path, string root)
        {
            var source = TestPath(path);

            var candidate = source.WikiRoot();

            Assert.That(candidate, Does.EndWith(root), "Path differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", true)]
        [TestCase("Samples\\Sample.wiki\\S2", true)]
        [TestCase("Samples\\Sample2", true)]
        [TestCase("Samples", false)]
        public void AttachmentPath(string path, bool found)
        {
            var source = TestPath(path);

            var candidate = source.AttachmentPath();

            if (found)
            {
                Assert.That(candidate, Does.EndWith(".attachments"));
            }
            else
            {
                Assert.That(candidate, Is.Null);
            }
        }

        private string TestPath(string path)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, path);
        }
    }
}