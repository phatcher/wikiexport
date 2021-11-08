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
            ExportDate = DateTime.UtcNow;
        }

        public DateTime ExportDate { get; set; }

        public void Export(ExportOptions options)
        {
            // Tidy up options info
            options.Tidy();

            // Now validate them
            if (!options.Validate(out var error))
            {
                throw logger.LogRaiseException(error);
            }

            // Work out what to call the target
            var targetFile = options.SelectTargetFile();

            // Ensure that the directory exists
            if (!Directory.Exists(options.TargetPath))
            {
                Directory.CreateDirectory(options.TargetPath);
            }

            logger.LogInformation($"Creating output directory: ${options.TargetPath}");

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
            writer.WriteLine($"title: {options.DocumentTitle()}");
            writer.WriteLine($"author: {options.Author}");
            writer.WriteLine($"date: {ExportDate:d MMMM yyyy}");
            if (options.TableOfContents)
            {
                writer.WriteLine("toc: yes");
            }

            writer.WriteLine("---");
        }

        private void FixAttachmentReferences(string fileName, string targetFile, ExportOptions options)
        {
            // Find the wiki root (options or just walk until we find a .attachments folder?)
            var attachmentSourcePath = options.SourcePath.AttachmentPath();

            var attachmentTargetPath = Path.Combine(options.TargetPath, targetFile + "-attachments");
            logger.LogInformation($"Attachments will be output to ${attachmentTargetPath}");

            // Get the data
            var source = File.ReadAllText(fileName);

            // Fix up the references, plus copy the files
            var fixer = new AttachmentFixer(attachmentSourcePath, attachmentTargetPath, options.RetainCaption);
            var result = fixer.Fix(source);

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
                throw logger.LogRaiseException($"No {file}.md in '{path}'");
            }

            if (options.AutoHeader && level > 0)
            {
                if (options.AppendixProcessing && file.IsAppendix())
                {
                    // Force the level so we get the appendix heading correct
                    level = options.AppendixHeadingLevel;
                }
                var heading = options.FileHeading(file, level);
                writer.WriteLine(heading);
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
            writer.WriteLine(string.Empty);

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
                throw logger.LogRaiseException($"No .order file in '{path}'");
            }

            var files = File.ReadAllLines(orderFile);
            foreach (var file in files)
            {
                AddWikiFile(writer, path, file, level, options);
            }
        }

        private class AttachmentFixer
        {
            private readonly string sourcePath;
            private readonly string targetPath;
            private readonly bool retainCaption;

            const string attachmentFinder = @"\[(?<caption>.+)\]\((?<path>/.attachments)/(?<name>.+)\)";

            public AttachmentFixer(string sourcePath, string targetPath, bool retainCaption)
            {
                this.sourcePath = sourcePath;
                this.targetPath = targetPath;
                this.retainCaption = retainCaption;
            }

            public string Fix(string source)
            {
                // Fix up the references
                var result = Regex.Replace(source, attachmentFinder, Replace);

                return result;
            }

            public string Replace(Match match)
            {
                var attachmentName = match.Groups["name"].Value;
                var caption = match.Groups["caption"].Value;

                // Decode it so we can find it
                // Can have %20 etc
                var fileName = HttpUtility.UrlDecode(attachmentName);
                var targetName = Path.Combine(targetPath, fileName);

                // Copy the file
                var targetDirectory = Path.GetDirectoryName(targetName);
                // Handles images in subfolders of the root attachments folder
                Directory.CreateDirectory(targetDirectory);
                File.Copy(Path.Combine(sourcePath, fileName), targetName, true);

                // And change the path to relative to where we want to be with the encoded name
                var directory = new DirectoryInfo(targetPath);
                return $"[{caption}]({Path.Combine(directory.Name, attachmentName)})";
            }
        }
    }
}