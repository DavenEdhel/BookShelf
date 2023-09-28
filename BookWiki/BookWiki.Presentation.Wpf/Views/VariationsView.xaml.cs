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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookWiki.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for VariationsView.xaml
    /// </summary>
    public partial class VariationsView : UserControl
    {
        public VariationsView()
        {
            InitializeComponent();
        }

        public VariationsView(string title, string[] variations)
        {
            InitializeComponent();

            Set(title, variations);
        }

        public void Set(string title, string root, string[] endings)
        {
            Set(title, endings.Select(ending => $"{root}{ending}").ToArray());
        }

        public void Set(string title, string[] variations)
        {
            InitializeComponent();

            Title.Text = title;
            Variations.Children.Clear();
            foreach (var variation in variations)
            {
                var v = new TextBox();
                v.BorderBrush = Brushes.LightGray;
                v.VerticalAlignment = VerticalAlignment.Stretch;
                v.Background = Brushes.White;
                v.FontFamily = new FontFamily("Times New Roman");
                v.FontSize = 16;
                v.Text = variation;
                Variations.Children.Add(v);
            }
        }

        public IEnumerable<string> TextVariations
        {
            get
            {
                foreach (TextBox variationsChild in Variations.Children)
                {
                    yield return variationsChild.Text;
                }
            }
        }

        public event EventHandler<EventArgs> Choosen = (sender, args) => { };

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Choosen(this, EventArgs.Empty);
        }
    }
}
