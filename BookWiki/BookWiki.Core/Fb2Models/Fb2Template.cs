using System.Collections.Generic;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2Template
    {
        public IBase64 Cover { get; set; }

        public string Annotation { get; set; }

        public string Title { get; set; }

        public List<IAbsolutePath> Chapters { get; set; } = new List<IAbsolutePath>();

        public void CompileToFolder(IAbsolutePath path)
        {
            var content = new Fb2TemplateString()
            {
                Title = Title,
                Annotation = Annotation,
                Cover = Cover ?? new AutodetectedCover(path),
                Body = new Fb2BookContent(Chapters).Value
            }.Value;

            new Fb2File(path, Title, content).Save();
        }
    }
}