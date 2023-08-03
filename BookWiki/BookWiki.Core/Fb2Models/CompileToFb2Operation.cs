using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Fb2Models
{
    public class CompileToFb2Operation
    {
        private readonly IRelativePath _novel;
        private readonly bool _changeNameOfChapterToNameOfFile;
        private readonly AbsoluteDirectoryPath _directoryPath;

        public CompileToFb2Operation(IRelativePath novel, IRootPath root)
        {
            _novel = novel;

            BookTitle = new NovelTitleShort(_novel).Value;

            _directoryPath = new AbsoluteDirectoryPath(root, novel);

            SelectedNovels = new IAbsolutePath[]
            {
                novel.AbsolutePath(root)
            };
        }

        public string BookTitle { get; set; }

        public bool ChangeNameOfChapterToNameOfFile { get; set; }

        public IEnumerable<IAbsolutePath> SelectedNovels { get; set; }

        public void Execute()
        {
            var content = new Fb2TemplateString()
            {
                Title = BookTitle,
                Annotation = "Для тестового ознакомления",
                Body = new Fb2BookContent(SelectedNovels.ToList())
                {
                    Chapter = (novel) => new Fb2ChapterV2(novel)
                        .Make(x =>
                        {
                            if (ChangeNameOfChapterToNameOfFile)
                            {
                                x.Title = (novelPath, novelContent) =>
                                    new CenteredFb2Paragraph(new NovelTitleShort(novelPath).Value);
                            }
                        })
                }.Value
            }.Value;

            new Fb2File(_directoryPath, BookTitle, content).Save();

            _directoryPath.OpenInExplorer();

        }
    }
}