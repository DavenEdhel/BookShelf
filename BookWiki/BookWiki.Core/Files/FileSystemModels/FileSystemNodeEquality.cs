using System;
using System.Linq;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class FileSystemNodeEquality
    {
        private readonly IFileSystemNode _first;
        private readonly IFileSystemNode _second;

        public Equality EqualityState
        {
            get
            {
                if (_first.Path.EqualsTo(_second.Path))
                {
                    return 0;
                }

                if (UnderSameDirectory(_first, _second))
                {
                    return (Equality)String.Compare(_first.Path.Parts.Last().PlainText, _second.Path.Parts.Last().PlainText, StringComparison.Ordinal);
                }

                for (int i = 0; i < _first.Path.Parts.Count(); i++)
                {
                    var fPart = _first.Path.Parts.ElementAt(i);
                    var sPart = _second.Path.Parts.ElementAtOrDefault(i);

                    if (sPart == null)
                    {
                        return Equality.FirstGreater;
                    }
                    else
                    {
                        if (fPart.PlainText.Equals(sPart.PlainText))
                        {
                            continue;
                        }
                        else
                        {
                            return (Equality)String.Compare(fPart.PlainText, sPart.PlainText, StringComparison.Ordinal);
                        }
                    }
                }

                return Equality.FirstLower;

                bool UnderSameDirectory(IFileSystemNode n1, IFileSystemNode n2)
                {
                    if (n1.Path.Parts.Count() != n2.Path.Parts.Count())
                    {
                        return false;
                    }

                    for (int i = 0; i < n1.Path.Parts.Count() - 1; i++)
                    {
                        var p1 = n1.Path.Parts.ElementAt(i);
                        var p2 = n2.Path.Parts.ElementAt(i);

                        if (p1.PlainText != p2.PlainText)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        public FileSystemNodeEquality(IFileSystemNode first, IFileSystemNode second)
        {
            _first = first;
            _second = second;
        }
    }
}