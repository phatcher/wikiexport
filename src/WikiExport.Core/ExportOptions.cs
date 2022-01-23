using CommandLine;

using Microsoft.Extensions.Logging;

namespace WikiExport
{
    /// <summary>
    /// Options for exporting Azure DevOps wikis
    /// </summary>
    public class ExportOptions
    {
        public ExportOptions()
        {
            AutoHeader = true;
            AutoLevel = true;
            AppendixProcessing = true;
            AppendixHeadingLevel = 6;
            TableOfContents = true;
            TitleFormat = "{project} {title}";
            Logging = LogLevel.Warning;
        }

        /// <summary>
        /// Get or set the project name, defaults to the source name less .wiki
        /// </summary>
        [Option('p', "project", Required = false, HelpText = "Project name, defaults to the source name less .wiki")]
        public string Project { get; set; }

        /// <summary>
        /// Get whether to include the project in the title
        /// </summary>
        public bool ProjectInTitle => TitleFormat.Contains("{project}") || TitleFormat.Contains("{0}");

        /// <summary>
        /// Gets or sets the title format
        /// </summary>
        [Option("titleFormat", Default = "{project} {title}", Required = false, HelpText = "Allows specification of different order + special characters")]
        public string TitleFormat { get; set; }

        /// <summary>
        /// Get or set the document title, default to source file name if not specified
        /// </summary>
        [Option("title", Required = false, HelpText = "Document title to use, defaults to source file name")]
        public string Title { get; set; }

        /// <summary>
        /// Get or set the author name, defaults to the source name less .wiki
        /// </summary>
        [Option('a', "author", Required = false, HelpText = "Author name")]
        public string Author { get; set; }

        /// <summary>
        /// Get or set the source path we are processing
        /// </summary>
        [Option('s', "source", Required = true, HelpText = "Source path to process")]
        public string SourcePath { get; set; }

        /// <summary>
        /// Get or set the source file we are processing, defaults to entire directory if not specified
        /// </summary>
        [Option('f', "file", Required = false, HelpText = "Source file to process, defaults to entire directory if not specified")]
        public string SourceFile { get; set; }

        /// <summary>
        /// Get or set the target path we will use
        /// </summary>
        [Option('t', "target", Required = true, HelpText = "Target path to output to")]
        public string TargetPath { get; set; }

        /// <summary>
        /// Get or set the target file we will use, defaults to the source file if not present
        /// </summary>
        [Option('u', "targetFile", Required = false, HelpText = "Target file, defaults to Document Title if not specified")]
        public string TargetFile { get; set; }

        /// <summary>
        /// Gets or sets whether we add a heading for each content file (except for the top-most file)
        /// </summary>
        [Option('h', "autoheading", Default = true, Required = false, HelpText = "Whether we add a heading for each content file (except for the top-most file)")]
        public bool AutoHeader { get; set; }

        /// <summary>
        /// Gets or sets whether we add a heading for the including file (except for the top-most file)
        /// </summary>
        [Option('l', "autolevel", Default = true, Required = false, HelpText = "Whether to adjust the markdown headings according to the nesting level")]
        public bool AutoLevel { get; set; }

        /// <summary>
        /// Gets or sets whether we retain attachment captions
        /// </summary>
        [Option('c', "retainCaption", Default = false, Required = false, HelpText = "Retain attachment captions")]
        public bool RetainCaption { get; set; }

        /// <summary>
        /// Gets or sets whether we automatically detect appendices in the wiki
        /// </summary>
        [Option("appendix", Default = true, Required = false, HelpText = "Whether we automatically detect/process appendices")]
        public bool AppendixProcessing { get; set; }

        /// <summary>
        /// Gets or sets the heading level at which appendices start.
        /// </summary>
        [Option("appendixLevel", Default = 6, Required = false, HelpText = "What heading level do appendices start from")]
        public int AppendixHeadingLevel { get; set; }

        /// <summary>
        /// Gets or set whether we want a table of contents
        /// </summary>
        [Option("toc", Default = true, Required = false, HelpText = "Whether we want a table of contents")]
        public bool TableOfContents { get; set; }

        /// <summary>
        /// Get or set the logging level
        /// </summary>
        [Option("log", Default = LogLevel.Warning, Required = false, HelpText = "Set the logging level")]
        public LogLevel Logging { get; set; }
    }
}