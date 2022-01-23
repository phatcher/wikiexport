using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace WikiExport
{
    public class AttachmentFixer
    {
        private readonly string sourcePath;
        private readonly string targetPath;
        private readonly bool retainCaption;

        // TODO: Worthwhile having this as a static compiled regex?
        const string attachmentFinder = @"\[(?<caption>.+)\]\((?<path>.*/?.attachments)/(?<name>.+)\)";

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
            var caption = retainCaption ? match.Groups["caption"].Value : string.Empty;

            // Decode it so we can find it
            // Can have %20 etc
            var fileName = HttpUtility.UrlDecode(attachmentName);
            var targetName = Path.Combine(targetPath, fileName);

            // Copy the file
            var targetDirectory = Path.GetDirectoryName(targetName);

            // Handles images in subfolders of the attachments folder
            Directory.CreateDirectory(targetDirectory);
            File.Copy(Path.Combine(sourcePath, fileName), targetName, true);

            // And change the path to relative to where we want to be with the encoded name
            var directory = new DirectoryInfo(targetPath);
            return $"[{caption}]({directory.Name}/{attachmentName})";
        }
    }
}