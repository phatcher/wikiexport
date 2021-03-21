using CommandLine;

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
            ProjectInTitle = true;
            ReplaceHyphen = true;
        }

        /// <summary>
        /// Get or set the project name, defaults to the source name less .wiki
        /// </summary>
        [Option('p', "project", Required = false, HelpText = "Project name")]
        public string Project { get; set; }

        [Option("projectInTitle", Required = false, HelpText = "Whether to include the project in the title")]
        public bool ProjectInTitle { get; set; }

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
        [Option('t', "target", Required = true, HelpText = "Target path")]
        public string TargetPath { get; set; }

        /// <summary>
        /// Get or set the target file we will use, defaults to the source file if not present
        /// </summary>
        [Option('u', "targetFile", Required = false, HelpText = "Target file, defaults to Document Title if not specified")]
        public string TargetFile { get; set; }

        /// <summary>
        /// Gets or sets whether we add a heading for each content file (except for the top-most file)
        /// </summary>
        [Option('h', "autoheading", Required = false, HelpText = "Whether we add a heading for each content file (except for the top-most file)")]
        public bool AutoHeader { get; set; }

        /// <summary>
        /// Gets or sets whether we add a heading for the including file (except for the top-most file)
        /// </summary>
        [Option('l', "autolevel", Required = false, HelpText = "Whether to adjust the markdown headings according to the nesting level")]
        public bool AutoLevel { get; set; }

        /// <summary>
        /// Gets or sets whether we retain attachment captions
        /// </summary>
        [Option('c', "retainCaption", Required = false, HelpText = "Retain attachment captions")]
        public bool RetainCaption { get; set; }
        
        [Option('y', "replaceHyphen", Required = false, HelpText = "Whether we replace hyphens in the document title/file name")]
        public bool ReplaceHyphen { get; set; }
    }
}