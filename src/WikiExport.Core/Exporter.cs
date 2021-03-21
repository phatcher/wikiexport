using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

using Microsoft.Extensions.Logging;

namespace WikiExport
{
    public class Exporter
    {
        private readonly ILogger logger;

        public Exporter(ILogger<Exporter> logger)
        {
            this.logger = logger;
        }

        public void Export(ExportOptions options)
        {
            if (!Directory.Exists(options.SourcePath))
            {
                logger.LogError($"Source path '{options.SourcePath}' does not exist");
                return;
            }

            if (string.IsNullOrEmpty(options.TargetPath))
            {
                logger.LogError("Target path not specified");
                return;
            }

            if (options.TargetPath == options.SourcePath)
            {
                logger.LogError("Source and target paths may not be the same");
                return;
            }

            // Work out what to call the target
            var targetFile = options.SelectTargetFile();

            // Ensure that the directory exists
            Directory.CreateDirectory(options.TargetPath);

            // And start processing
            var fileName = options.TargetPath.WikiFileName(targetFile);
            using (var writer = File.CreateText(fileName))
            {
                // TODO: Option whether to put in main file or separate.
                WriteMetadata(writer, options);

                // Process on behalf of the the directory or the file, rest happens recursively
                if (string.IsNullOrEmpty(options.SourceFile))
                {
                    // Pass level one as we need headings for each file 
                    AddWikiDirectory(writer, options.SourcePath, 1, options);
                }
                else
                {
                    // Pass level 0 as the topmost file is the document heading
                    AddWikiFile(writer, options.SourcePath, options.SourceFile, 0, options);
                }
            }

            // TODO: Option whether to put in main file or separate.
            //var metaFileName = Path.Combine(options.TargetPath, targetFile + "-Metadata.md");
            //using (var writer = File.CreateText(metaFileName))
            //{
            //    WriteMetadata(writer, options);
            //}

            FixAttachmentReferences(fileName, targetFile, options);
        }

        private void WriteMetadata(StreamWriter writer, ExportOptions options)
        {
            // Write out some metadata
            writer.WriteLine("---");
            // NOTE: Always replace hyphens here, displayed in document.
            writer.WriteLine($"title: {options.DocumentTitle().Replace('-', ' ')}");
            writer.WriteLine($"author: {options.Author}");
            writer.WriteLine($"date: {DateTime.UtcNow:d MMMM yyyy}");
            // TODO: Option
            writer.WriteLine("toc: yes");
            writer.WriteLine("---");
        }

        private void FixAttachmentReferences(string fileName, string targetFile, ExportOptions options)
        {
            const string attachmentFinder = @"\[(?<caption>.+)\]\((?<path>/.attachments)/(?<name>.+)\)";

            // Find the wiki root (options or just walk until we find a .attachments folder?)
            var attachmentSourcePath = options.SourcePath.AttachmentPath();

            var attachmentTargetPath = Path.Combine(options.TargetPath, targetFile + "-attachments");

            var fixer = new AttachmentFixer(attachmentSourcePath, attachmentTargetPath, options.RetainCaption);

            // Get the data
            var source = File.ReadAllText(fileName);

            // Fix up the references, plus copy the files
            var result = Regex.Replace(source, attachmentFinder, fixer.Replace);

            // And write back the updated references
            File.WriteAllText(fileName, result);
        }

        /// <summary>
        /// Appends a wiki file to the output, also recursively processes any subdirectories
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <param name="level"></param>
        /// <param name="options"></param>
        private void AddWikiFile(StreamWriter writer, string path, string file, int level, ExportOptions options)
        {
            // Note we add the md this late to simplify the directory handling
            var sourceFile = path.WikiFileName(file);
            if (!File.Exists(sourceFile))
            {
                logger.LogError($"No {file}.md in '{path}'");
                return;
            }

            if (options.AutoHeader && level > 0)
            {
                writer.WriteLine($"{new string('#', level)} {file.Replace("-", " ")}");
            }

            foreach (var line in File.ReadAllLines(sourceFile))
            {
                if (line.StartsWith("#") && options.AutoLevel && level > 1)
                {
                    // Causes the header to indent
                    writer.Write(new string('#', level-1));
                }
                writer.WriteLine(line);
            }

            // Buffer line - options?
            writer.WriteLine("");

            // See if we have a sub directory associated
            if (Directory.Exists(Path.Combine(path, file)))
            {
                // Ok, so process recursively
                AddWikiDirectory(writer, Path.Combine(path, file), ++level, options);
            }
        }

        /// <summary>
        /// Appends a wiki directory to the output, also recursively processes any subdirectories
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="path"></param>
        /// <param name="level"></param>
        /// <param name="options"></param>
        private void AddWikiDirectory(StreamWriter writer, string path, int level, ExportOptions options)
        {
            var orderFile = path.WikiOrderingFile();
            if (!File.Exists(orderFile))
            {
                logger.LogError($"No .order file in '{path}'");
                return;
            }

            var files = File.ReadAllLines(orderFile);
            foreach (var file in files)
            {
                AddWikiFile(writer, path, file, level, options);
            }
        }

        private class AttachmentFixer
        {
            private string sourcePath;
            private string targetPath;
            private bool retainCaption;
            private bool found;
            private DirectoryInfo directory;

            public AttachmentFixer(string sourcePath, string targetPath, bool retainCaption)
            {
                this.sourcePath = sourcePath;
                this.targetPath = targetPath;
                this.retainCaption = retainCaption;
                found = false;
            }

            public string Replace(Match match)
            {
                if (found == false)
                {
                    // Ensure the target directory exists
                    // Create on first capture only - avoids empty directory if no attachments
                    directory = Directory.CreateDirectory(targetPath);
                    found = true;
                }

                var attachmentName = match.Groups["name"].Value;
                var caption = retainCaption ? match.Groups["caption"].Value : string.Empty;

                // Copy it so we can find it
                // Can have %20 etc
                var fileName = HttpUtility.UrlDecode(attachmentName);
                File.Copy(Path.Combine(sourcePath, fileName), Path.Combine(targetPath, fileName), true);

                // And change the path to relative to where we are
                return $"[{caption}]({Path.Combine(directory.Name, attachmentName)})";
            }
        }
    }
}