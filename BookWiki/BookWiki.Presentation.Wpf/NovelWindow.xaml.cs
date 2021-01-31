using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for NovelWindow.xaml
    /// </summary>
    public partial class NovelWindow : Window, IErrorsCollectionV2
    {
        // todo: life replace " with «»

        private IRelativePath _fabel = new FolderPath(@"Рассказы\Сказки\Сказка 1. Рога, копыта да хвосты.n");
        private IRelativePath _aboutTime = new FolderPath(@"Материалы\Письмена Атлины\Книга 6. Время.n");
        private IRelativePath _summerNight = new FolderPath(@"Рассказы\Идеи\Летняя Ночь.n");
        private IRelativePath _currentlyLoaded = null;
        private LifeSpellCheckV2 _lifeSpellCheck;

        public NovelWindow(IRelativePath novel)
        {
            InitializeComponent();

            Rtb.FontFamily = new FontFamily("Times New Roman");
            Rtb.FontSize = 16;
            Rtb.Language = XmlLanguage.GetLanguage("ru");

            _lifeSpellCheck = new LifeSpellCheckV2(Rtb.Document, this, new RussianDictionarySpellChecker());

            LoadContent(novel);
        }

        private void Content_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _lifeSpellCheck.TextChangedInside(Rtb.CaretPosition.Paragraph);
        }

        private async void SpellCheckButton(object sender, RoutedEventArgs e)
        {
            var startOfInline = Rtb.Document.Blocks.FirstBlock.CastTo<Paragraph>().Inlines.FirstInline.ContentStart;
            

            for (int i = 0; i < 1000; i++)
            {
                // Content.GetPositionFromPoint()
                var position = Rtb.CaretPosition.GetCharacterRect(LogicalDirection.Forward);

                var start = i;
                //var end = int.Parse(End.Text);

                var x1 = Rtb.Document.ContentStart.GetPositionAtOffset(start).GetCharacterRect(LogicalDirection.Forward);
                var x2 = Rtb.Document.ContentStart.GetPositionAtOffset(start + 1).GetCharacterRect(LogicalDirection.Forward);

                if (x2.X > x1.X)
                {
                    var r = new Rectangle()
                    {
                        Width = x2.X - x1.X,
                        Height = 1,
                        Stroke = Brushes.LightSeaGreen,
                        Stretch = Stretch.Fill
                    };

                    Canvas.SetTop(r, x1.Bottom);
                    Canvas.SetLeft(r, x1.X);

                    SpellCheckBox.Children.Add(r);
                }

                await Task.Delay(1000);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            var c = new DocumentFlowContentFromRichTextBox(Rtb);

            var formattedContent = new FormattedContentFromDocumentFlow(c);

            var file = new ContentFolder(_currentlyLoaded.AbsolutePath(BookShelf.Instance.RootPath));
            file.Save(formattedContent);
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {
            LoadContent(_fabel);
        }

        private void LoadContent2(object sender, RoutedEventArgs e)
        {
            LoadContent(_aboutTime);
        }

        private void LoadContent(IRelativePath path)
        {
            _currentlyLoaded = path;

            var novel = new Novel(path, BookShelf.Instance.RootPath);

            var rtf = new DocumentFlowContentFromTextAndFormat(novel);

            Rtb.Document.Blocks.Clear();

            foreach (var rtfParagraph in rtf.Paragraphs)
            {
                var paragraph = new Paragraph();

                switch (rtfParagraph.FormattingStyle)
                {
                    case TextStyle.Centered:
                        paragraph.TextAlignment = TextAlignment.Center;
                        break;
                    case TextStyle.Right:
                        paragraph.TextAlignment = TextAlignment.Right;
                        break;
                }

                foreach (var rtfParagraphInline in rtfParagraph.Inlines)
                {
                    var inline = new Run(rtfParagraphInline.Text.PlainText);

                    switch (rtfParagraphInline.TextStyle)
                    {
                        case TextStyle.Bold:
                            paragraph.Inlines.Add(new Bold(inline));
                            break;
                        case TextStyle.Italic:
                            paragraph.Inlines.Add(new Italic(inline));
                            break;
                        case TextStyle.BoldAndItalic:
                            paragraph.Inlines.Add(new Italic(new Bold(inline)));
                            break;
                        default:
                            paragraph.Inlines.Add(inline);
                            break;
                    }
                }

                Rtb.Document.Blocks.Add(paragraph);
            }
        }

        private void LoadContent3(object sender, RoutedEventArgs e)
        {
            LoadContent(_summerNight);
        }

        private void ToRight(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Right;
            }
        }

        private void ToLeft(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Left;
            }
        }

        private void ToCenter(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Center;
            }
        }

        public void Add(ISubstring error)
        {
            var x1 = Rtb.Document.ContentStart.GetPositionAtOffset(error.StartIndex).GetCharacterRect(LogicalDirection.Forward);
            var x2 = Rtb.Document.ContentStart.GetPositionAtOffset(error.EndIndex).GetCharacterRect(LogicalDirection.Forward);

            if (x2.X > x1.X)
            {
                var r = new Rectangle()
                {
                    Width = x2.X - x1.X,
                    Height = 1,
                    Stroke = Brushes.LightSeaGreen,
                    Stretch = Stretch.Fill
                };

                Canvas.SetTop(r, x1.Bottom);
                Canvas.SetLeft(r, x1.X);

                SpellCheckBox.Children.Add(r);
            }
        }

        public void RemoveAll()
        {
            SpellCheckBox.Children.Clear();
        }
    }
}
