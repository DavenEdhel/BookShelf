using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.LibraryModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Presentation.Apple.Views.Controls;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Path = System.IO.Path;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FileSystemWindow : Window
    {
        private bool _closeRequested = false;
        private bool _doNotStoreSession = false;

        public FileSystemWindow()
        {
            InitializeComponent();

            FileSystemScroll.Content = new FileSystemView(BookShelf.Instance.Root);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_closeRequested)
            {
                _closeRequested = true;

                if (_doNotStoreSession == false)
                {
                    BookShelf.Instance.StoreSession();

                    _doNotStoreSession = true;
                }

                BookShelf.Instance.CloseAllAsync();

                e.Cancel = true;
                base.OnClosing(e);
            }
            else
            {
                base.OnClosing(e);
            }
        }

        public void Restore()
        {
            _closeRequested = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (BookShelf.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                return;
            }
        }
    }
}