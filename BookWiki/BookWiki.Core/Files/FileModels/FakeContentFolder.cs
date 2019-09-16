using System.Net.Http.Headers;
using System.Text;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;

namespace BookWiki.Core
{
    public class FakeContentFolder : IContentFolder
    {
        public IAbsolutePath Source { get; } = new PathFake("1");

        public void Save(IText text)
        {

        }

        public void Save(ISequence<ITextInfo> novelFormat)
        {

        }

        public virtual IText LoadText()
        {
            var result = new StringBuilder();

            for (int i = 0; i < 25; i++)
            {
                result.Append(ArticlePart.CreateText(ContentHelper.GenerateParagraph()));
            }

            return new StringText(result.ToString());
        }

        public virtual ISequence<ITextInfo> LoadFormat()
        {
            return new ArraySequence<ITextInfo>(new ITextInfo[0]);
        }
    }
}