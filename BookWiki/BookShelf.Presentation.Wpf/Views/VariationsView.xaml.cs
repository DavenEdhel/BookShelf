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
using BookWiki.Core;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

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

        public void SetAutomatically(string word, Sex sex)
        {
            InitializeComponent();

            Title.Text = word;
            Variations.Children.Clear();

            try
            {
                var last = word.Last();
                var ending = new WordEnding(word).Value;
                var beforeLast = word.Reverse().Skip(1).First();

                if ((sex == Sex.M || sex == Sex.W) && ending.IsIn("а", "я"))
                {
                    if (ending == "а")
                    {
                        if (beforeLast.IsIn('ш'))
                        {
                            Set(word, new WordMainPart(word).Value, new[] { "а", "и", "е", "у", "ой", "е" });
                        }
                        else if (beforeLast.IsIn('н', 'в'))
                        {
                            Set(word, new WordMainPart(word).Value, new[] { "а", "ы", "е", "у", "ой", "е" });
                        }
                        else
                        {
                            Set(word, new WordMainPart(word).Value, new[] { "а", "и", "е", "у", "ой", "е" });
                        }
                    }

                    if (ending == "я")
                    {
                        Set(word, new WordMainPart(word).Value, new[] { "я", "и", "е", "ю", "ей", "е" });
                    }
                }
                else if ((sex == Sex.M || sex == Sex.U) && ending.IsIn("", "о", "е"))
                {
                    if (ending == "")
                    {
                        Set(word, new WordMainPart(word).Value, new[] { "", "а", "у", "", "ом", "е" });
                    }

                    if (ending == "о")
                    {
                        Set(word, new WordMainPart(word).Value, new[] { "о", "а", "у", "о", "ом", "е" });
                    }

                    if (ending == "е")
                    {
                        Set(word, new WordMainPart(word).Value, new[] { "е", "я", "ю", "е", "ем", "е" });
                    }
                }
                else if ((sex == Sex.U && ending == ""))
                {
                    Set(word, new WordMainPart(word).Value, new[] { "", "и", "и", "", "ю", "и" });
                }
                else if ((sex == Sex.W && last == 'ь'))
                {
                    Set(word, word.Substring(0, word.Length - 1), new[] { "ь", "и", "и", "ь", "ью", "и" });
                }
                else
                {
                    Set(word, word, new[] { "", "", "", "", "", "" });
                }
            }
            catch (Exception e)
            {
            }
        }

        private void CreateVariation(string text)
        {
            var v = new TextBox();
            v.BorderBrush = Brushes.LightGray;
            v.VerticalAlignment = VerticalAlignment.Stretch;
            v.Background = Brushes.White;
            v.FontFamily = new FontFamily("Times New Roman");
            v.FontSize = 16;
            v.Text = text;
            Variations.Children.Add(v);
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
