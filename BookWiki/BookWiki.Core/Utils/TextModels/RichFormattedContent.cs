using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;

namespace BookWiki.Core
{
    public class RichFormattedContent
    {
        private readonly IText _content;
        private readonly ISequence<ITextInfo> _format;

        public RichFormattedContent(IText content, ISequence<ITextInfo> format)
        {
            _content = content;
            _format = format;
        }

        public IText GetHtmlString()
        {
            var stringBuilder = new StringBuilder();

            var offset = 0;
            foreach (var p in _content.PlainText.Split(new []{'\n'}))
            {
                if (IsBreak(p, out int l))
                {
                    stringBuilder.Append("<p>\n<br>\n</p>");
                    offset += l;
                    continue;
                }

                var formats = _format.Where(x =>
                {
                    if (x.Range.In(new SimpleRange(p.Length, offset)).PartiallyOrCompletely())
                    {
                        var content = x.Range.GetPlainText(_content).PlainText;

                        if (IsBreak(content, out _) == false)
                        {
                            if ((p + "\n").Contains(content))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                               ;
                }).ToArray();

                bool IsBreak(string c, out int letters)
                {
                    if (c == "\n" || c == "\r")
                    {
                        letters = 1;

                        return true;
                    }

                    if (c == "\n\r" || c == "\r\n")
                    {
                        letters = 2;

                        return true;
                    }

                    if (string.IsNullOrWhiteSpace(c))
                    {
                        letters = 1;

                        return true;
                    }

                    letters = 0;

                    return false;
                }

                var formatted = p;

                foreach (var textInfo in formats)
                {
                    switch (textInfo.Style)
                    {
                        case TextStyle.Bold:
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset + textInfo.Range.Length - offset, maximum: formatted.Length), "</strong>");
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset - offset, minimum: 0), "<strong>");
                            break;
                        case TextStyle.Italic:
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset + textInfo.Range.Length - offset, maximum: formatted.Length), "</em>");
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset - offset, minimum: 0), "<em>");
                            break;
                        case TextStyle.BoldAndItalic:
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset + textInfo.Range.Length - offset, maximum: formatted.Length), "</strong>");
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset + textInfo.Range.Length - offset, maximum: formatted.Length), "</em>");
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset - offset, minimum: 0), "<strong>");
                            formatted = formatted.Insert(new Number(textInfo.Range.Offset - offset, minimum: 0), "<em>");
                            break;
                    }
                }

                var format = formats.FirstOrDefault(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right);

                if (format != null)
                {
                    stringBuilder.Append($"<p style=\"text-align: {StyleToCss(format)};\">");
                }
                else
                {
                    stringBuilder.Append("<p>");
                }

                stringBuilder.Append(formatted);
                stringBuilder.Append("</p>\n");

                string StyleToCss(ITextInfo f)
                {
                    switch (f.Style)
                    {
                        case TextStyle.Centered:
                            return "center";
                        case TextStyle.Right:
                            return "right";
                        default: throw new Exception();
                    }
                }

                offset += p.Length + 1;
            }

            return new StringText(stringBuilder.ToString());
        }
    }
}