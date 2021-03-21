using System.IO;

namespace WikiExport
{
    public static class ExportOptionsExtensions
    {
        /// <summary>
        /// Determine the project name based on the options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string ProjectName(this ExportOptions options)
        {
            if (!string.IsNullOrEmpty(options.Project))
            {
                return options.Project;
            }

            // Lets walk up
            var root = options.SourcePath.WikiRoot();
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            var directory = new DirectoryInfo(root);

            // Splits off the .wiki if it exists
            return directory.Name.Split('.')[0];
        }

        /// <summary>
        /// Determine the document title based on the options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string DocumentTitle(this ExportOptions options)
        {
            if (!string.IsNullOrEmpty(options.Title))
            {
                return options.Title;
            }

            var value = string.Empty;
            if (!string.IsNullOrEmpty(options.SourceFile))
            {
                // Explicitly specified
                value = options.SourceFile;
            }
            else if (options.SourcePath == options.SourcePath.WikiRoot())
            {
                // We are the root, but if we don't have ProjectInTitle we won't have anything without this
                if (!options.ProjectInTitle)
                {
                    value = options.ProjectName();
                }
            }
            else
            {
                // Not the root, so should be named ok
                value = new DirectoryInfo(options.SourcePath).Name;
            }

            if (options.ProjectInTitle)
            {
                value = $"{options.ProjectName()}-{value}";
            }

            value = options.ReplaceHyphen ? value.Replace('-', ' ') : value;

            return value.Trim();
        }

        /// <summary>
        /// Determine the target file name to use based on the options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string SelectTargetFile(this ExportOptions options)
        {
            string value;

            if (!string.IsNullOrEmpty(options.TargetFile))
            {
                // Use the specified value
                value = options.TargetFile;
            }
            else
            {
                // Otherwise name it according to the project info
                // TODO: Should be an option to pick source file or DocumentTitle ?
                value = options.DocumentTitle();
            }

            return value;
        }

        /// <summary>
        /// Determine the root of the wiki.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The root of the wiki or null if we don't think this is a wiki</returns>
        /// <remarks>Uses the existence of an .order file to determine wiki directories</remarks>
        public static string WikiRoot(this string path)
        {
            string root = null;

            var candidate = new DirectoryInfo(path);
            while (candidate.FullName.HasOrderFile())
            {
                root = candidate.FullName;
                candidate = candidate.Parent;
            }

            return root;
        }

        /// <summary>
        /// Checks if a path contains a .order file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool HasOrderFile(this string path)
        {
            var result = File.Exists(path.WikiOrderingFile());

            return result;
        }

        /// <summary>
        /// Construct the name of the wiki order file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string WikiOrderingFile(this string path)
        {
            return Path.Combine(path, ".order");
        }

        /// <summary>
        /// Construct the name of a wiki content file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string WikiFileName(this string path, string file)
        {
            return Path.Combine(path, $"{file}.md");
        }
        
        /// <summary>
        /// Determine the attachments path for a wiki directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string AttachmentPath(this string path)
        {
            var root = path.WikiRoot();
            if (root == null)
            {
                // Push the problem up one layer
                return null;
            }

            // Note that the directory might not exist if we have no attachments
            return Path.Combine(root, ".attachments");
        }
    }
}