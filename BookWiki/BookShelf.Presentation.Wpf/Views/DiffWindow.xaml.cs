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
using BookWiki.Core.Fb2Models;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DiffWindow.xaml
    /// </summary>
    public partial class DiffWindow : Window
    {
        public DiffWindow()
        {
            InitializeComponent();
        }

        private readonly AbsoluteDirectoryPath _directoryPath;
        private readonly IRelativePath _node;
        private readonly Novel[] _novels;

        public DiffWindow(IRelativePath node)
        {
            _node = node;
            InitializeComponent();

            _directoryPath = new AbsoluteDirectoryPath(BookShelf.Instance.RootPath, node);
            Title = new NovelTitle(_node).PlainText;

            BookTitle.Text = new NovelTitleShort(node).Value;

            _novels = new FileSystemNode(_directoryPath).InnerNodes.Where(x => x.IsContentFolder).Select(x => BookShelf.Instance.Read(x.Path.RelativePath(BookShelf.Instance.RootPath))).SortAndReturn().ToArray();

            foreach (var novel in _novels)
            {
                var checkbox = new CheckBox();
                checkbox.Content = new TextBox()
                {
                    Text = new NovelTitle(novel.Source).PlainText
                };

                checkbox.Tag = novel;

                checkbox.IsChecked =  node.EqualsTo(novel.Source);

                Chapters.Children.Add(checkbox);
            }
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
            new CompileToFb2Operation(_node, BookShelf.Instance.RootPath)
            {
                ChangeNameOfChapterToNameOfFile = CheckedNovels().Count() > 1,
                SelectedNovels = CheckedNovels().Select(x => x.Source.AbsolutePath(BookShelf.Instance.RootPath)).ToArray()
            }.Execute();

            //var content = new Fb2TemplateString()
            //{
            //    Title = BookTitle.Text,
            //    Annotation = "Для тестового ознакомления",
            //    Body = new Fb2BookContent(CheckedNovels().ToArray().Select(x => x.Source.AbsolutePath(BookShelf.Instance.RootPath)).ToList())
            //    {
            //        Chapter = (novel) => new Fb2ChapterV2(novel)
            //        {
            //            Title = (novelPath, novelContent) => new CenteredFb2Paragraph(new NovelTitleShort(novelPath).Value)
            //        }
            //    }.Value
            //}.Value;

            //new Fb2File(_directoryPath, BookTitle.Text, content).Save();

            //_directoryPath.OpenInExplorer();
        }
    }
}
