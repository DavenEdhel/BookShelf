using System.IO;
using System.Reflection;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.MockDataModels
{
    public class LibraryInitializationOperation
    {
        private readonly IRootPath _root;

        public LibraryInitializationOperation(IRootPath root)
        {
            _root = root;
        }

        public void Execute()
        {
            var content = new StringText(GetContentAsString("Text.txt"));

            var novel = new NovelFake()
            {
                Content = content,
                Format = TextFormat.ParseFrom(GetContentAsString("Format.json"), content)
            };

            new NovelFolder(_root, "Новелла").Save(novel);
        }

        public static string GetContentAsString(string resourceName)
        {
            var assembly = typeof(LibraryInitializationOperation).GetTypeInfo().Assembly;
            var contentPath = $"{assembly.GetName().Name}.ContentGenerator.{resourceName}";
            var stream = assembly.GetManifestResourceStream(contentPath);
            using (var streamReader = new StreamReader(stream))
            {
                var text = streamReader.ReadToEnd();
                return text;
            }
        }
    }
}