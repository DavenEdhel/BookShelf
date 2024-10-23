using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public enum Sex
    {
        M,
        W,
        U
    }

    public partial class GenerateVariationsWindow : Window
    {
        private bool initialized = false;

        public GenerateVariationsWindow()
        {
            InitializeComponent();
        }

        public string[] ResultTags { get; set; } = Array.Empty<string>();

        public void SetInitialName(string articleTitle)
        {
            Root.Text = articleTitle;

            foreach (var form in forms)
            {
                var v = new VariationsView();
                v.Title.Foreground = form.Item2 == Sex.U
                    ? Brushes.Black
                    : form.Item2 == Sex.W
                        ? Brushes.PaleVioletRed
                        : Brushes.DarkBlue;
                v.Choosen += OnChoosed;
                Variations.Children.Add(v);
            }

            uW = new VariationsView();
            uW.Title.Foreground = Brushes.PaleVioletRed;
            uW.Choosen += OnChoosed;
            AutoVariations.Children.Add(uW);

            uM = new VariationsView();
            uM.Title.Foreground = Brushes.DarkBlue;
            uM.Choosen += OnChoosed;
            AutoVariations.Children.Add(uM);

            uU = new VariationsView();
            uU.Title.Foreground = Brushes.Black;
            uU.Choosen += OnChoosed;
            AutoVariations.Children.Add(uU);

            initialized = true;

            FillIn();
        }

        private List<(string, Sex, string[])> forms = new List<(string, Sex, string[])>()
        {
            ("Неизменяемая", Sex.U, new []{"", "", "", "", "", ""}),
            ("1 ж.р. 1", Sex.W, new []{"а", "и", "е", "у", "ой", "е"}),
            ("1 ж.р. 2",Sex.W, new []{"я", "ы", "е", "ю", "ёй", "е"}),
            ("3 ж 1",Sex.W, new []{"", "и", "и", "", "ю", "и"}),
            ("1 м",Sex.M, new []{ "а", "и", "е", "ю", "ей", "е"}),
            ("2 м 1",Sex.M, new []{"", "а", "у", "", "ом", "е"}),
            ("2 м 2",Sex.M, new []{"", "я", "ю", "", "ём", "е"}),
            ("2 ср 1",Sex.U, new []{"о", "а", "у", "о", "ом", "е"}),
            ("2 ср 2",Sex.U, new []{"е", "я", "ю", "е", "ем", "е"}),
        };

        private List<(string, Sex, string[])> forms2 = new List<(string, Sex, string[])>()
        {
            ("Неизменяемая", Sex.U, new []{"", "", "", "", "", ""}),
            ("1 ж.р. 1", Sex.W, new []{"а", "и", "е", "у", "ой", "е"}),
            ("1 ж.р. 2",Sex.W, new []{"я", "ы", "е", "ю", "ёй", "е"}),
            ("3 ж 1",Sex.W, new []{"", "и", "и", "", "ю", "и"}),
            ("1 м",Sex.M, new []{ "а", "и", "е", "ю", "ей", "е"}),
            ("2 м 1",Sex.M, new []{"", "а", "у", "", "ом", "е"}),
            ("2 м 2",Sex.M, new []{"", "я", "ю", "", "ём", "е"}),
            ("2 ср 1",Sex.U, new []{"о", "а", "у", "о", "ом", "е"}),
            ("2 ср 2",Sex.U, new []{"е", "я", "ю", "е", "ем", "е"}),
        };

        private VariationsView uW;
        private VariationsView uM;
        private VariationsView uU;

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            if (initialized == false)
            {
                return;
            }

            FillIn();
        }

        private void FillIn()
        {
            var root = Root.Text;
            if (Root.Text.Contains(" ") == false)
            {
                for (int i = 0; i < forms.Count; i++)
                {
                    if (i == 0)
                    {
                        Variations.Children[i].CastTo<VariationsView>().Set(forms[i].Item1, root, forms[i].Item3);
                    }
                    else
                    {
                        Variations.Children[i].CastTo<VariationsView>().Set(forms[i].Item1, new WordMainPart(root).Value, forms[i].Item3);
                    }
                }
            }

            uW.SetAutomatically(root, Sex.W);
            uM.SetAutomatically(root, Sex.M);
            uU.SetAutomatically(root, Sex.U);
        }

        private void OnChoosed(object sender, EventArgs e)
        {
            var v = sender.CastTo<VariationsView>();
            ResultTags = v.TextVariations.ToArray();
            Close();
        }
    }
}
