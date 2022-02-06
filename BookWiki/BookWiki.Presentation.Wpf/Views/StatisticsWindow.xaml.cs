using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookWiki.Core;
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

            _novels = _node.InnerNodes.Where(x => x.IsContentFolder).Select(x => BookShelf.Instance.Read(x.Path.RelativePath(BookShelf.Instance.RootPath))).ToArray();

            var toInclude = new NodeFolder(node.Path).Load();

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

            Recalculate(null, null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            new NodeFolder(_node.Path).Save(CheckedNovels().Select(x => x.Source));

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
    }
}
