using System;
using System.Linq;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Files.PathModels
{
    public class Extension : IExtension
    {
        public string PlainText => _plainText.Value;

        public NodeType Type => _type.Value;

        private readonly IProperty<string> _plainText;
        private readonly IProperty<NodeType> _type;

        public Extension(NodeType node)
        {
            _type = new CachedValue<NodeType>(node);
            _plainText = new CachedValue<string>(GetStringFromNodeType);
        }

        public Extension(IPartsSequence parts) : this()
        {
            _plainText = new CachedValue<string>(() =>
            {
                var p = parts.Last().SplitBy('.');

                if (p.Count() == 1)
                {
                    return string.Empty;
                }

                return p.Last().PlainText;
            });
        }

        public Extension(ITextRange textRange) : this()
        {
            _plainText = new CachedValue<string>(() => textRange.PlainText);
        }

        private Extension()
        {
            _type = new CachedValue<NodeType>(GetNodeType);
        }

        private NodeType GetNodeType()
        {
            switch (PlainText)
            {
                case "":
                {
                    return NodeType.Directory;
                }
                case "n":
                {
                    return NodeType.Novel;
                }
                case "ar":
                {
                    return NodeType.Article;
                }
                case "m":
                {
                    return NodeType.Article;
                }
                default:
                {
                    return NodeType.Unknown;
                }
            }
        }

        private string GetStringFromNodeType()
        {
            switch (Type)
            {
                case NodeType.Directory:
                    return string.Empty;
                case NodeType.Novel:
                    return "n";
                case NodeType.Article:
                    return "ar";
                case NodeType.Map:
                    return "m";
                default:
                    return string.Empty;
            }
        }
    }
}