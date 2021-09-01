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

        [TestCase("My-Title", 1, "# My Title")]
        [TestCase("Conceptual-Level%3A-Behaviour", 2, "## Conceptual Level: Behaviour")]
        [TestCase("Non%2DFunctional-Requirements", 3, "### Non-Functional Requirements")]
        [TestCase("Appendix-A%3A-Bibliography", 1, "# Bibliography")]
        public void FileHeading(string name, int level, string expected)
        {
            var options = new ExportOptions
            {
            };

            var candidate = options.FileHeading(name, level);

            Assert.That(candidate, Is.EqualTo(expected), "File heading differs");
        }

        [TestCase("My-Title", 1, "# My Title")]
        [TestCase("Conceptual-Level%3A-Behaviour", 2, "## Conceptual Level: Behaviour")]
        [TestCase("Non%2DFunctional-Requirements", 3, "### Non-Functional Requirements")]
        [TestCase("Appendix-A%3A-Bibliography", 1, "# Appendix A: Bibliography")]
        public void FileHeadingIgnoreAppendix(string name, int level, string expected)
        {
            var options = new ExportOptions
            {
                AppendixProcessing = false
            };

            var candidate = options.FileHeading(name, level);

            Assert.That(candidate, Is.EqualTo(expected), "File heading differs");
        }

        [TestCase("My Title", "My-Title")]
        [TestCase("Conceptual Level: Behaviour", "Conceptual-Level%3A-Behaviour")]
        [TestCase("Non-Functional Requirements", "Non%2DFunctional-Requirements")]
        public void WikiEncode(string name, string expected)
        {
            var candidate = name.WikiEncode();

            Assert.That(candidate, Is.EqualTo(expected), "Name differs");
        }

        [TestCase("C:\\Sample.wiki\\S2-Foo\\S3: Bar", "C:\\Sample.wiki\\S2%2DFoo\\S3%3A-Bar")]
        public void FixupPath(string name, string expected)
        {
            var candidate = name.WikiEncode().FixupPath();

            Assert.That(candidate, Is.EqualTo(expected), "Name differs");
        }

        [TestCase("My-Title", "My Title")]
        [TestCase("Conceptual-Level%3A-Behaviour", "Conceptual Level: Behaviour")]
        [TestCase("Non%2DFunctional-Requirements", "Non-Functional Requirements")]
        public void WikiDecode(string name, string expected)
        {
            var candidate = name.WikiDecode();

            Assert.That(candidate, Is.EqualTo(expected), "Name differs");
        }

        [TestCase("My Title", "My Title")]
        [TestCase("Appendix Bibliography", "Bibliography")]
        [TestCase("Appendix: Bibliography", "Bibliography")]
        [TestCase("Appendix - Bibliography", "Bibliography")]
        [TestCase("Appendix A: Bibliography", "Bibliography")]
        public void AppendixName(string name, string expected)
        {
            var candidate = name.AppendixName();

            Assert.That(candidate, Is.EqualTo(expected), "Name differs");
        }

        [TestCase("Samples\\Sample.wiki\\S1\\SS1", null, "SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "SS1", "SS1")]
        [TestCase("Samples\\Sample.wiki\\S1", "My-Section", "My-Section")]
        [TestCase("Samples\\Sample.wiki\\S2", null, "S2")]
        [TestCase("Samples\\Sample.wiki", null, "")]
        [TestCase("Samples\\Sample2", null, "")]
        // This because we are not passing a proper wiki directory in
        [TestCase("Samples", null, "Samples")]
        public void DocumentTitleBaseFromWiki(string path, string file, string expected)
        {
            var options = new ExportOptions
            {
                SourcePath = TestPath(path),
                SourceFile = file
            };

            var candidate = options.DocumentTitleBase();

            Assert.That(candidate, Is.EqualTo(expected), "DocumentTitle name differs");
        }

        [Test]
        public void DocumentTitleBaseSpecified()
        {
            var expected = "Foo";

            var options = new ExportOptions
            {
                SourcePath = "Samples\\Sample.wiki\\S1",
                SourceFile = "My-Section",
                Title = expected
            };

            var candidate = options.DocumentTitleBase();

            Assert.That(candidate, Is.EqualTo(expected), "DocumentTitle name differs");
        }

        /// <remarks>Variations on using wiki-derived vs explicit title are covered above</remarks>
        [TestCase("P1-A", "My-Title", "", true, "P1 A My Title")]
        [TestCase("P1-A", "My-Title", "{0}: {1}", true, "P1 A: My Title")]
        [TestCase("P1-A", "My-Title", "", false, "My Title")]
        [TestCase("P1-A", "Conceptual-Level%3A-Behaviour", "", true, "P1 A Conceptual Level: Behaviour")]
        public void DocumentTitleFormatting(string projectName, string title, string format, bool includeProject, string expected)
        {
            var options = new ExportOptions
            {
                Project = projectName,
                Title = title,
                ProjectInTitle = includeProject,
            };

            if (!string.IsNullOrEmpty(format))
            {
                options.TitleFormat = format;
            }

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