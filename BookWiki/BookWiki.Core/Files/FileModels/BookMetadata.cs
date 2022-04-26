namespace BookWiki.Core.Files.FileModels
{
    public class BookMetadata
    {
        public BookMetadata(NodeFolder.StatisticsSchema statistics)
        {
            Title = statistics.Title;
            Annotation = statistics.Annotation;
        }

        public string Annotation { get; set; }

        public string Title { get; set; }
    }
}