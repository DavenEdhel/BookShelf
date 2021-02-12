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

        public FileSystemWindow()
        {
            InitializeComponent();

            //new NovelWindow(_summerNight).Show();

            FileSystemScroll.Content = new FileSystemView(BookShelf.Instance.Root);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (Window currentWindow in Application.Current.Windows)
            {
                if (currentWindow is NovelWindow novelWindow)
                {
                    try
                    {
                        novelWindow.Save();

                        novelWindow.Close();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($"Something went wrong with {novelWindow.Novel.Name}", exception.ToString());
                    }
                }
            }

            base.OnClosing(e);
        }

        private async void SpellCheckButton(object sender, RoutedEventArgs e)
        {
            
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            
        }

        private void LoadContent(object sender, RoutedEventArgs e)
        {
        }

        private void LoadContent2(object sender, RoutedEventArgs e)
        {
            
        }

        private void LoadContent(IRelativePath path)
        {
            
        }

        private void LoadContent3(object sender, RoutedEventArgs e)
        {
            
        }

        private void ToRight(object sender, RoutedEventArgs e)
        {
            
        }

        private void ToLeft(object sender, RoutedEventArgs e)
        {
            
        }

        private void ToCenter(object sender, RoutedEventArgs e)
        {
            
        }
    }
}