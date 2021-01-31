using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Search;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Keurig.Tests.Common.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class IndexSequenceV2Tests
    {
        [Test]
        public void ShouldReturnIndexesOfAllX()
        {
            var text = "012XX5X78X";

            var s = new IndexesOfCharacters(text, 'X').ToArray();

            Claim.Equal(s.Length, 4);
            Claim.Equal(text[s[0]], 'X');
            Claim.Equal(text[s[1]], 'X');
            Claim.Equal(text[s[2]], 'X');
            Claim.Equal(text[s[3]], 'X');
        }

        [Test]
        public void ShouldSplitToIntervalsCorrectly()
        {
            var text = "Рога, копыта да хвосты";

            var s = new IntervalsBetweenCharacters(text, TextParts.NotALetterOrNumber).ToArray();

            Claim.Equal(text.Substring(s[0]), "Рога");
            Claim.Equal(text.Substring(s[1]), "копыта");
            Claim.Equal(text.Substring(s[2]), "да");
            Claim.Equal(text.Substring(s[3]), "хвосты");
        }

        [Test]
        public void ShouldSplitToSubstringsCorrectly()
        {
            var items = new List<OffsetText>()
            {
                new OffsetText()
                {
                    Offset = 100,
                    Text = "123 456 "
                },
                new OffsetText()
                {
                    Offset = 110,
                    Text = "789"
                },
                new OffsetText()
                {
                    Offset = 120,
                    Text = " 123"
                },
                new OffsetText()
                {
                    Offset = 130,
                    Text = " "
                },
                new OffsetText()
                {
                    Offset = 140,
                    Text = "456 "
                },
                new OffsetText()
                {
                    Offset = 150,
                    Text = "123"
                },
                new OffsetText()
                {
                    Offset = 160,
                    Text = "4"
                },
                new OffsetText()
                {
                    Offset = 170,
                    Text = "56"
                },
            };

            var substrings = new PunctuationSeparatedEnumerationV2(items).ToArray();

            Claim.Equal(substrings[0].Text, "123");
            Claim.Equal(substrings[1].Text, "456");
            Claim.Equal(substrings[2].Text, "789");
            Claim.Equal(substrings[3].Text, "123");
            Claim.Equal(substrings[4].Text, "456");
            Claim.Equal(substrings[5].Text, "123456");
        }

        [Test]
        public void ShouldSplitToSubstringsCorrectly2()
        {
            var items = new List<OffsetText>()
            {
                new OffsetText()
                {
                    Offset = 150,
                    Text = "123"
                },
                new OffsetText()
                {
                    Offset = 160,
                    Text = "4"
                },
                new OffsetText()
                {
                    Offset = 170,
                    Text = "56"
                },
            };

            var substrings = new PunctuationSeparatedEnumerationV2(items).ToArray();

            Claim.Equal(substrings.Length, 1);
            Claim.Equal(substrings[0].Text, "123456");
        }

        [Test]
        public async Task SpellcheckShouldWork()
        {
            var lex = new WordCollectionFromLex(string.Empty, new FakeFileProvider(new[]
            {
                "ааа",
                "аб",
                "абв"
            }));

            await lex.Load();

            var spellCheck = new RussianDictionarySpellChecker(lex);

            Claim.False(spellCheck.IsCorrect("аа"));
            Claim.True(spellCheck.IsCorrect("аб"));
            Claim.True(spellCheck.IsCorrect("ааа"));
            Claim.False(spellCheck.IsCorrect("аааа"));
        }
    }



    public class DocumentFlowTests
    {
        private string _textWithRN = @"..\..\..\Глава 01. Впереди шторм.n";
        private string _textWithN = @"..\..\..\Книга 6. Время.n";

        [Test]
        public void SingleLineTextShouldContainOneParagraph()
        {
            var d = new DocumentFlowContentFromTextAndFormat(new StringText("123456789"), new EmptySequence<ITextInfo>());

            Claim.Equal(d.Paragraphs.Length, 1);

            Claim.Equal(d.Paragraphs[0].Inlines[0].Text.PlainText, "123456789");
        }

        [Test]
        public void EmptyTextShouldContainOneParagraph()
        {
            var d = new DocumentFlowContentFromTextAndFormat(new StringText(""), new EmptySequence<ITextInfo>());

            Claim.Equal(d.Paragraphs.Length, 1);

            Claim.Equal(d.Paragraphs[0].Inlines[0].Text.PlainText, "");
        }

        [Test]
        public void ParagraphsShouldBeSplitWithN()
        {
            var text = new StringText("123\n456\n789");

            var format = new ArraySequence<ITextInfo>(new ITextInfo[]
            {
                new TextInfo(text.Substring(0, 3), TextStyle.Bold),
                new TextInfo(text.Substring(0, 5), TextStyle.Right),
                new TextInfo(text.Substring(8, 2), TextStyle.Centered), 
            });

            var d = new DocumentFlowContentFromTextAndFormat(text, format);

            Claim.Equal(d.Paragraphs.Length, 3);

            Claim.Equal(d.Paragraphs[0].Inlines[0].Text.PlainText, "123");
            Claim.Equal(d.Paragraphs[1].Inlines[0].Text.PlainText, "456");
            Claim.Equal(d.Paragraphs[2].Inlines[0].Text.PlainText, "789");

            Claim.Equal(d.Paragraphs[0].FormattingStyle, TextStyle.Right);
            Claim.Equal(d.Paragraphs[1].FormattingStyle, TextStyle.Right);
            Claim.Equal(d.Paragraphs[2].FormattingStyle, TextStyle.Centered);
        }

        [Test]
        public void ParagraphsShouldBeSplitWithNAndR()
        {
            var text = new StringText("123\r\n456\r\n789");
            var format = new ArraySequence<ITextInfo>(new ITextInfo[]
            {
                new TextInfo(text.Substring(0, 3), TextStyle.Bold),
                new TextInfo(text.Substring(0, 3), TextStyle.Right),
                new TextInfo(text.Substring(10, 2), TextStyle.Centered), 
            });

            var d = new DocumentFlowContentFromTextAndFormat(text, format);

            Claim.Equal(d.Paragraphs.Length, 3);

            Claim.Equal(d.Paragraphs[0].Inlines[0].Text.PlainText, "123");
            Claim.Equal(d.Paragraphs[1].Inlines[0].Text.PlainText, "456");
            Claim.Equal(d.Paragraphs[2].Inlines[0].Text.PlainText, "789");

            Claim.Equal(d.Paragraphs[0].FormattingStyle, TextStyle.Right);
            Claim.Equal(d.Paragraphs[1].FormattingStyle, TextStyle.None);
            Claim.Equal(d.Paragraphs[2].FormattingStyle, TextStyle.Centered);
        }

        [Test]
        public void ParagraphsWithEmptyParagraphShouldBeParsed()
        {
            var text = new StringText("123\r\n\n789");
            var format = new ArraySequence<ITextInfo>(new ITextInfo[]
            {
                new TextInfo(text.Substring(0, 5), TextStyle.Right),
            });

            var d = new DocumentFlowContentFromTextAndFormat(text, format);

            Claim.Equal(d.Paragraphs.Length, 3);

            Claim.Equal(d.Paragraphs[0].Inlines[0].Text.PlainText, "123");
            Claim.Equal(d.Paragraphs[1].Inlines[0].Text.PlainText, "");
            Claim.Equal(d.Paragraphs[2].Inlines[0].Text.PlainText, "789");

            Claim.Equal(d.Paragraphs[0].FormattingStyle, TextStyle.Right);
            Claim.Equal(d.Paragraphs[1].FormattingStyle, TextStyle.None);
            Claim.Equal(d.Paragraphs[2].FormattingStyle, TextStyle.None);
        }

        [Test]
        public void InlinesShouldBeExtracted()
        {
            var text = new StringText("12\n123456");

            var format = new ArraySequence<ITextInfo>(new ITextInfo[]
            {
                new TextInfo(text.Substring(1, 3), TextStyle.Italic), //2\n1
                new TextInfo(text.Substring(5, 1), TextStyle.Bold), //3
                new TextInfo(text.Substring(6, 2), TextStyle.Italic), //45
            });

            var p = new ParagraphFromTextRange(text, text.Substring(3, 6), format);

            Claim.Equal(p.Inlines.Length, 5);
            Claim.Equal(p.Inlines[0].Text.PlainText, "1");
            Claim.Equal(p.Inlines[1].Text.PlainText, "2");
            Claim.Equal(p.Inlines[2].Text.PlainText, "3");
            Claim.Equal(p.Inlines[3].Text.PlainText, "45");
            Claim.Equal(p.Inlines[4].Text.PlainText, "6");

            Claim.Equal(p.Inlines[0].TextStyle, TextStyle.Italic);
            Claim.Equal(p.Inlines[1].TextStyle, TextStyle.None);
            Claim.Equal(p.Inlines[2].TextStyle, TextStyle.Bold);
            Claim.Equal(p.Inlines[3].TextStyle, TextStyle.Italic);
            Claim.Equal(p.Inlines[4].TextStyle, TextStyle.None);
        }

        [Test]
        public void RangeShouldBeSplittedCorrectly()
        {
            var origin = new StringText("01234567890123456789");
            var frame = origin.Substring(3, 11); //34567890123

            var format = new ArraySequence<ITextRange>(new ITextRange[]
            {
                origin.Substring(2, 3), //234
                origin.Substring(0, 16), //01234567890123456
                origin.Substring(4, 5) //45678
            });

            // 3 4 5678 90123

            var items = new TextIntervalEnumeration(origin, frame, format).ToArray(); 

            Claim.Equal(items.Length, 4);
            Claim.Equal(items[0].PlainText, "3");
            Claim.Equal(items[1].PlainText, "4");
            Claim.Equal(items[2].PlainText, "5678");
            Claim.Equal(items[3].PlainText, "90123");
        }

        [Test]
        public void ParagraphsParsingWithN()
        {
            var path = Path.GetFullPath(_textWithRN);

            var novel = new ContentFolder(new FolderPath(path));
            var text = novel.LoadText();
            var format = novel.LoadFormat();

            var textsStyles = format.Where(x => x.Style == TextStyle.BoldAndItalic || x.Style == TextStyle.Bold || x.Style == TextStyle.Italic).Select(x => text.Substring(x.Range.Offset, x.Range.Length)).ToArray();
            var paragraphsStyles = format.Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).Where(x => x.Range.Length > 2).Select(x => text.Substring(x.Range.Offset, x.Range.Length)).ToArray();

            var document = new DocumentFlowContentFromTextAndFormat(text, format);

            var paragraphs = document.Paragraphs.Where(x => x.FormattingStyle != TextStyle.None).ToArray();
            var inlines = document.Paragraphs.SelectMany(x => x.Inlines).Where(x => x.TextStyle != TextStyle.None).ToArray();

            Claim.Equal(inlines.Length, textsStyles.Length);
            Claim.Equal(paragraphs.Length, paragraphsStyles.Length);

            for (int i = 0; i < inlines.Length; i++)
            {
                Claim.True(textsStyles[i].PlainText == inlines[i].Text.PlainText);
            }

            var formattedContent = new FormattedContentFromDocumentFlow(document);

            var inlines2 = formattedContent.Format.Where(x => x.Style == TextStyle.BoldAndItalic || x.Style == TextStyle.Bold || x.Style == TextStyle.Italic).Select(x => text.Substring(x.Range.Offset, x.Range.Length)).ToArray();

            for (int i = 0; i < textsStyles.Length; i++)
            {
                Claim.Equal(textsStyles[i].PlainText.Replace("\n", ""), inlines2[i].PlainText.Replace("\n", ""));
            }
        }

        [Test]
        public void ParagraphsParsingWithRAndN()
        {
            var path = Path.GetFullPath(_textWithN);

            var novel = new ContentFolder(new FolderPath(path));
            var text = novel.LoadText();
            var format = novel.LoadFormat();

            var texts = format.Select(x => text.Substring(x.Range.Offset, x.Range.Length)).ToArray();

            var document = new DocumentFlowContentFromTextAndFormat(text, format);

            var inlines = document.Paragraphs.SelectMany(x => x.Inlines).Where(x => x.TextStyle != TextStyle.None).ToArray();

            Claim.Equal(inlines.Length, 4);

            Claim.Equal(inlines[0].Text.PlainText, texts[0].PlainText.Replace("\n", ""));
            Claim.Equal(inlines[1].Text.PlainText, texts[1].PlainText.Replace("\n", ""));
            Claim.Equal(inlines[2].Text.PlainText, texts[2].PlainText.Replace("\n", ""));
            Claim.Equal(inlines[3].Text.PlainText, texts[3].PlainText.Replace("\n", ""));

            var formattedContent = new FormattedContentFromDocumentFlow(document);

            var inlines2 = formattedContent.Format.Where(x => x.Style == TextStyle.BoldAndItalic || x.Style == TextStyle.Bold || x.Style == TextStyle.Italic).Select(x => formattedContent.Content.Substring(x.Range.Offset, x.Range.Length)).ToArray();

            for (int i = 0; i < texts.Length; i++)
            {
                Claim.Equal(texts[i].PlainText.Replace("\n", ""), inlines2[i].PlainText.Replace("\n", ""));
            }
        }
    }
}