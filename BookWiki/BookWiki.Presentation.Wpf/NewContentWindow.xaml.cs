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

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for NewContentWindow.xaml
    /// </summary>
    public partial class NewContentWindow : Window
    {
        public NewContentWindow()
        {
            InitializeComponent();
        }

        public IExtension Extension { get; set; }

        private void NewNovel(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Extension = new Extension(NodeType.Novel);

            Close();
        }

        private void NewFolder(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Extension = new Extension(NodeType.Directory);

            Close();
        }
    }
}
