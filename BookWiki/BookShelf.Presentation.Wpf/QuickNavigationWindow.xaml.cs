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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for QuickNavigationWindow.xaml
    /// </summary>
    public partial class QuickNavigationWindow : Window
    {
        private int _lineSelected = 0;

        public QuickNavigationWindow()
        {
            InitializeComponent();

            SearchBar.Focus();

            RefreshSuggestions();

            RefreshSelection();
        }

        private void SearchBarKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                OpenSelected();
                Close();
                return;
            }
        }

        private void OpenSelected()
        {
            if (_lineSelected < 1)
            {
                return;
            }

            var novelPath = Items.Children[_lineSelected - 1].CastTo<TextBlock>().Tag.CastTo<IAbsolutePath>();

            BookShelf.Instance.Open(novelPath.RelativePath(BookShelf.Instance.RootPath));
        }

        private void RefreshSuggestions()
        {
            Items.Children.Clear();

            var allLeafs = BookShelf.Instance.Search.Execute(SearchBar.Text);

            var i = 1;
            foreach (var fileSystemNode in allLeafs)
            {
                Items.Children.Add(CreateSuggestion(fileSystemNode.Path, i));
                i++;
            }
        }

        private TextBlock CreateSuggestion(IAbsolutePath path, int i)
        {
            return new TextBlock()
            {
                Text = new NovelTitle(path).PlainText,
                Tag = path,
                FontFamily = new FontFamily("Times New Roman"),
                FontSize = 12,
                Background = i == _lineSelected ? Brushes.DarkGray : Brushes.White
            };
        }

        private void RefreshSelection()
        {
            if (_lineSelected < 1)
            {
                _lineSelected = 1;
            }

            if (_lineSelected > Items.Children.Count)
            {
                _lineSelected = Items.Children.Count;
            }

            var i = 1;
            foreach (TextBlock itemsChild in Items.Children)
            {
                if (i == _lineSelected)
                {
                    itemsChild.Background = Brushes.DarkGray;
                }
                else
                {
                    itemsChild.Background = Brushes.White;
                }
                
                i++;
            }
        }

        private void QuickNavigationWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void SearchBar_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                _lineSelected++;
                RefreshSelection();
                return;
            }

            if (e.Key == Key.Up)
            {
                _lineSelected--;
                RefreshSelection();
                return;
            }
        }

        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSuggestions();
        }
    }
}
