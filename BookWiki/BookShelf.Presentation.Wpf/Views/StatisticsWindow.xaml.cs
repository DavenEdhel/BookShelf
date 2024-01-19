using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BookWiki.Core;
using BookWiki.Core.Fb2Models;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private readonly IFileSystemNode _node;
        private readonly Novel[] _novels;

        public StatisticsWindow(IFileSystemNode node)
        {
            _node = node;
            InitializeComponent();

            Title = node.Path.Name.PlainText;

            _novels = _node.InnerNodes.Where(x => x.IsContentFolder).Select(x => BooksApplication.Instance.Read(x.Path.RelativePath(BooksApplication.Instance.RootPath))).ToArray();

            var statistics = new NodeFolder(node.Path);

            var toInclude = statistics.Load();

            foreach (var novel in _novels)
            {
                var checkbox = new CheckBox();
                checkbox.Content = new TextBox()
                {
                    Text = new NovelTitle(novel.Source).PlainText
                };

                checkbox.Tag = novel;
                checkbox.Checked += Recalculate;
                checkbox.Unchecked += Recalculate;

                checkbox.IsChecked = toInclude.Any(x => x.EqualsTo(novel.Source));

                Chapters.Children.Add(checkbox);
            }

            var bookMetadata = statistics.LoadMetadata();

            BookTitle.Text = bookMetadata.Title;
            BookAnnotation.Text = bookMetadata.Annotation;

            var coverPath = new AutodetectCoverFilePath(node.Path);

            if (coverPath.Value != null)
            {
                Cover.Source = new BitmapImage(new Uri(coverPath.Value.FullPath));
            }

            Recalculate(null, null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            new NodeFolder(_node.Path).Save(CheckedNovels().Select(x => x.Source), BookTitle.Text, BookAnnotation.Text);

            base.OnClosing(e);
        }

        private void Recalculate(object sender, RoutedEventArgs e)
        {
            TotalSizeCharacters.Text = $"Итоговый размер: {GetTotalSize()}k";

            CommentsSize.Text = $"Комментарии: {GetTotalComments()}k";
        }

        private double GetTotalSize()
        {
            var total = 0;
            foreach (var novel in CheckedNovels())
            {
                total += novel.Content.PlainText.Length;
            }

            return total;
        }

        private double GetTotalComments()
        {
            var total = 0;
            foreach (var novel in CheckedNovels())
            {
                total += novel.Comments.PlainText.Length;
            }

            return total;
        }

        private IEnumerable<Novel> CheckedNovels()
        {
            foreach (CheckBox chaptersChild in Chapters.Children)
            {
                if (chaptersChild.IsChecked == true)
                {
                    yield return chaptersChild.Tag.CastTo<Novel>();
                }
            }
        }

        private void GenerageFb2(object sender, RoutedEventArgs e)
        {
            new Fb2Template()
            {
                Annotation = BookAnnotation.Text,
                Title = BookTitle.Text,
                Chapters = CheckedNovels().ToArray().Select(x => x.Source.AbsolutePath(BooksApplication.Instance.RootPath)).ToList()
            }.CompileToFolder(_node.Path);

            MessageBox.Show("Книга скомпилирована", "Результат");

            _node.Path.OpenInExplorer();
        }
    }
}
