using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class NovelTitleShort : IString, IComparable
    {
        public NovelTitleShort(IPath path)
        {
            Value = path.Name.PlainText;
        }

        public string Value { get; }

        public int CompareTo(object obj)
        {
            if (obj is NovelTitleShort n)
            {
                if (ReferenceEquals(n, this)) return 0;
                if (ReferenceEquals(null, this)) return 1;

                if (Value.Equals(n.Value))
                {
                    return 0;
                }

                var origin = int.TryParse(Value.Split(' ').LastOrDefault(), out var _) ? Value : Value + " 01";
                var another = int.TryParse(n.Value.Split(' ').LastOrDefault(), out var _) ? n.Value : n.Value + " 01";

                return string.Compare(origin, another, StringComparison.Ordinal);
            }

            return -1;
        }
    }
}