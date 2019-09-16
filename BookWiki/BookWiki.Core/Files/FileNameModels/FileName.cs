using System.Linq;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.PathModels
{
    public class FileName : IFileName
    {
        private readonly ITextRange _fileNameWithExtension;
        private readonly IPartsSequence _fullPath;

        public FileName(IPartsSequence fullPath)
        {
            _fullPath = fullPath;
        }

        public FileName(string fileNameWithExtension)
        {
            _fileNameWithExtension = new SubstringText(fileNameWithExtension);
        }

        public FileName(ITextRange fileNameWithExtension)
        {
            _fileNameWithExtension = fileNameWithExtension;
        }

        public string PlainText
        {
            get
            {
                if (_fullPath != null)
                {
                    var lastPart = _fullPath.Last();

                    var name = GetNameByTextRange(lastPart);

                    return name;
                }

                if (_fileNameWithExtension != null)
                {
                    var name = GetNameByTextRange(_fileNameWithExtension);

                    return name;
                }

                return string.Empty;
            }
        }

        private string GetNameByTextRange(ITextRange lastPart)
        {
            var parts = lastPart.SplitBy('.');

            var last = parts.Last();

            if (new Extension(last).Type == NodeType.Unknown)
            {
                var name = string.Join(".", parts.Select(x => x.PlainText));

                return name;
            }
            else
            {
                var nameParts = parts.ExceptOf(last).ToArray();

                var name = string.Join(".", nameParts.Select(x => x.PlainText));

                return name;
            }
        }
    }
}