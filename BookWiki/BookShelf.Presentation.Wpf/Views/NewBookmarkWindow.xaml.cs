﻿using System;
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

namespace BookShelf.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for NewBookmarkWindow.xaml
    /// </summary>
    public partial class NewBookmarkWindow : Window
    {
        public NewBookmarkWindow()
        {
            InitializeComponent();
        }

        private void NewBookmark(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Close();
        }
    }
}
