using System.IO;

namespace BookWiki.Core.Files.PathModels
{
    public static class AbsolutePathExtensions
    {
        public static bool HasResource(this IAbsolutePath path) => File.Exists(path.FullPath);

        public static void OpenInExplorer(this IAbsolutePath path)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path.FullPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}