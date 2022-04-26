using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class NodeFolder
    {
        private const string Statistics = "Statistics.json";

        private readonly IFile _statistics;

        public NodeFolder(IAbsolutePath path)
        {
            _statistics = new TextFile(new FilePath(path, Statistics));

        }

        public void Save(IEnumerable<IRelativePath> include, string title, string annotation)
        {
            _statistics.Save(JsonConvert.SerializeObject(new StatisticsSchema()
            {
                RelativePaths = include.Select(x => x.FullPath).ToArray(),
                Title = title,
                Annotation = annotation
            }));
        }

        public class StatisticsSchema
        {
            public string[] RelativePaths { get; set; } = new string[0];

            public string Title { get; set; } = string.Empty;

            public string Annotation { get; set; } = string.Empty;
        }

        public IRelativePath[] Load()
        {
            var schema = JsonConvert.DeserializeObject<StatisticsSchema>(_statistics.Content) ?? new StatisticsSchema();

            return schema.RelativePaths.Select(x => new FolderPath(x)).ToArray();
        }

        public BookMetadata LoadMetadata()
        {
            var schema = JsonConvert.DeserializeObject<StatisticsSchema>(_statistics.Content) ?? new StatisticsSchema();

            return new BookMetadata(schema);
        }
    }
}