using System;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class AutostyleHtmlParagraph : IString
    {
        private readonly string _content;
        private readonly ITextInfo _format;

        public AutostyleHtmlParagraph(string content, ITextInfo format)
        {
            _content = content;
            _format = format;
        }

        public string Value
        {
            get
            {
                if (_format != null)
                {
                    switch (_format.Style)
                    {
                        case TextStyle.Centered:
                            return CenteredStyle(_content).Value;

                        case TextStyle.Right:
                            return RightStyle(_content).Value;

                        default:
                            return _content;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_content))
                    {
                        return "<br />";
                    }
                    else
                    {
                        return NoStyle(_content).Value;
                    }
                }
            }
        }

        public Func<string, IString> CenteredStyle { get; set; } = content => new WrappedText(new WrappedText(content, "p").Value, "title");

        public Func<string, IString> RightStyle { get; set; } = content => new RightStyledWrappedText(new WrappedText(content, "em").Value, "p ");

        public Func<string, IString> NoStyle { get; set; } = content => new WrappedText(content, "p");
    }

    public class AutostyleFb2Paragraph : IString
    {
        private readonly string _content;
        private readonly ITextInfo _format;

        public AutostyleFb2Paragraph(string content, ITextInfo format)
        {
            _content = content;
            _format = format;
        }

        public string Value
        {
            get
            {
                if (_format != null)
                {
                    switch (_format.Style)
                    {
                        case TextStyle.Centered:
                            return CenteredStyle(_content).Value;

                        case TextStyle.Right:
                            return RightStyle(_content).Value;

                        default:
                            return _content;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_content))
                    {
                        return "<br />";
                    }
                    else
                    {
                        return NoStyle(_content).Value;
                    }
                }
            }
        }

        public Func<string, IString> CenteredStyle { get; set; } = content => new WrappedText(new WrappedText(content, "p").Value, "title");

        public Func<string, IString> RightStyle { get; set; } = content => new WrappedText(new WrappedText(content, "emphasis").Value, "p");

        public Func<string, IString> NoStyle { get; set; } = content => new WrappedText(content, "p");
    }

    public class CenteredFb2Paragraph : IString
    {
        public CenteredFb2Paragraph(string content)
        {
            Value = new WrappedText(new WrappedText(content, "p").Value, "title").Value;
        }

        public string Value { get; }
    }
}