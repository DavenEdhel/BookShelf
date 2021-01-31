using System;

namespace BookWiki.Core.Utils
{
    public static class TextParts
    {
        public static Char[] NotALetterOrNumber = new Char[] {' ', '.', ',', '!', '?', ';', ':', '\n', '\r', '"', '»', '«', '-', '–', '…'};
    }
}