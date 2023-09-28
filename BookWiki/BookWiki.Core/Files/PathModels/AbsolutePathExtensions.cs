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

        public static void EnsureCreated(this IAbsolutePath path)
        {
            if (Directory.Exists(path.FullPath) == false)
            {
                Directory.CreateDirectory(path.FullPath);
            }
        }
    }
}