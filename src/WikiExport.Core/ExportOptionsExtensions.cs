using System;
using System.IO;
using System.Linq;
using System.Net;

namespace WikiExport
{
    public static class ExportOptionsExtensions
    {
        /// <summary>
        /// Tidy up option values
        /// </summary>
        /// <param name="options"></param>
        public static void Tidy(this ExportOptions options)
        {
            //options.SourcePath = options.SourcePath?.Replace(' ', '-');
            options.SourcePath = options.SourcePath?.WikiEncode().FixupPath();
            options.SourceFile = options.SourceFile?.WikiEncode();
        }

        /// <summary>
        /// Validate the option values
        /// </summary>
        /// <param name="options"></param>
        /// <param name="error"></param>
        /// <returns>true if no fatal errors, otherwise false</returns>
        public static bool Validate(this ExportOptions options, out string error)
        {
            error = string.Empty;
            if (!Directory.Exists(options.SourcePath))
            {
                error += $"Source path '{options.SourcePath}' does not exist\n";
            }

            if (string.IsNullOrEmpty(options.TargetPath))
            {
                error += "Target path not specified\n";
            }

            if (options.TargetPath == options.SourcePath)
            {
                error += "Source and target paths may not be the same\n";
            }
            else if (options.TargetPath.StartsWith(options.SourcePath))
            {
                error += "Target path may not be subdirectory of source path\n";

            }
            return error.Length == 0;
        }

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
            var value = options.DocumentTitleBase().WikiDecode();

            if (options.ProjectInTitle)
            {
                value = string.Format(options.TitleFormat, options.ProjectName().WikiDecode(), value);
            }

            return value.Trim();
        }

        /// <summary>
        /// Determine the base document title to use
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string DocumentTitleBase(this ExportOptions options)
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

            return value;
        }

        /// <summary>
        /// Determine the file heading to use.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <remarks>Not responsible for determining level for appendices, section processing does that.</remarks>
        public static string FileHeading(this ExportOptions options, string name, int level)
        {
            // Decode it, so we get to simple characters;
            var result = name.WikiDecode();

            // Either we don't need to process for appendices
            if (options.AppendixProcessing)
            {
                result = result.AppendixName();
            }

            return $"{new string('#', level)} {result}";
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
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

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

        /// <summary>
        /// Encode to the wiki file naming convention
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string WikiEncode(this string name)
        {
            var result = name;

            if (!string.IsNullOrEmpty(result))
            {
                // Handle special characters 
                result = WebUtility.UrlEncode(result);
                // Specific replaces for wiki names
                result = result?.Replace("-", "%2D");
                result = result?.Replace("+", "-");
            }

            return result;
        }

        /// <summary>
        /// Decode from the wiki file naming convention
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string WikiDecode(this string name)
        {
            var result = name;
            if (!string.IsNullOrEmpty(result))
            {
                // Replace the hyphens first so we don't confuse with %2D
                result = result?.Replace('-', ' ');
                result = WebUtility.UrlDecode(result);
            }

            return result;
        }

        /// <summary>
        /// Determine if we are an appendix
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsAppendix(this string name)
        {
            var x = name?.ToUpperInvariant();
            if (string.IsNullOrEmpty(x))
            {
                return false;
            }

            return x.StartsWith("APPENDIX");
        }

        /// <summary>
        /// Determine if we are an appendix section
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsAppendixSection(this string name)
        {
            var x = name?.ToUpperInvariant();
            if (string.IsNullOrEmpty(x))
            {
                return false;
            }

            return x.StartsWith("APPENDICES");
        }

        /// <summary>
        /// Get the name of an appendix from its title.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string AppendixName(this string name)
        {
            try
            {
                var ignore = new[] { ' ', ':', '-' };

                if (!name.IsAppendix())
                {
                    return name;
                }

                var posn = 8;
                while (posn < name.Length)
                {
                    var c = name[posn];
                    if (ignore.Contains(c))
                    {
                        posn++;
                        continue;
                    }

                    // Done if we find a letter/number
                    if (char.IsLetterOrDigit(c))
                    {
                        if (ignore.Contains(name[posn + 1]))
                        {
                            // ...unless followed by an ignore character
                            posn++;
                            continue;
                        }

                        break;
                    }
                }

                return name.Substring(posn);
            }
            catch (Exception)
            {
                // Don't crash, just return the name we started with
                return name;
            }
        }

        /// <summary>
        /// Fixes up a path after url encoding it.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FixupPath(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.IndexOf("%3A", StringComparison.InvariantCultureIgnoreCase) == 1)
                {
                    // Just replace the first %2D with a colon as it's a drive letter
                    value = $"{value[0]}:{value.Substring(4)}";
                }
                // So it works for path
                value = value?.Replace("%25", "%");
                value = value?.Replace("%5C", "\\");
                value = value?.Replace("%2F", "/");
            }

            return value;
        }
    }
}