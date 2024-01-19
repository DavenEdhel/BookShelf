using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for WordInfoWindow.xaml
    /// </summary>
    public partial class WordInfoWindow : Window
    {
        public WordInfoWindow(string word)
        {
            InitializeComponent();

            WordFromWiki.Navigated += ViewerWebBrowserControlView_Navigated;

            word = word.ToLower();

            WordTextBox.Text = word;
            AdjustedNameForSearch.Text = word;

            WordFromWiki.Navigate("https://ru.m.wiktionary.org/wiki/" + word);

            ParseMorfologyTable();
        }

        void ViewerWebBrowserControlView_Navigated(object sender, NavigationEventArgs e)
        {
            BrowserHandler.SetSilent(WordFromWiki, true); // make it silent
        }

        public static class BrowserHandler
        {
            private const string IWebBrowserAppGUID = "0002DF05-0000-0000-C000-000000000046";
            private const string IWebBrowser2GUID = "D30C1661-CDAF-11d0-8A3E-00C04FC9E26E";

            public static void SetSilent(System.Windows.Controls.WebBrowser browser, bool silent)
            {
                if (browser == null)
                    MessageBox.Show("No Internet Connection");

                // get an IWebBrowser2 from the document
                IOleServiceProvider sp = browser.Document as IOleServiceProvider;
                if (sp != null)
                {
                    Guid IID_IWebBrowserApp = new Guid(IWebBrowserAppGUID);
                    Guid IID_IWebBrowser2 = new Guid(IWebBrowser2GUID);

                    object webBrowser;
                    sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                    if (webBrowser != null)
                    {
                        webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                    }
                }
            }

        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);


        }

        private void AdjustedNameForSearch_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WordFromWiki.Navigate("https://ru.m.wiktionary.org/wiki/" + AdjustedNameForSearch.Text);

                ParseMorfologyTable();
            }
        }

        private async void ParseMorfologyTable()
        {
            var c = new HttpClient();
            var result = await c.GetAsync("https://ru.m.wiktionary.org/wiki/" + AdjustedNameForSearch.Text);
            var html = await result.Content.ReadAsStringAsync();

            var match = Regex.Match(html, "class=\"[^\"]*\\bmorfotable\\b[^\"]*\"[^>]*>");
            if (match.Success == false)
            {
                return;
            }

            var end = html.IndexOf("</table>", match.Index + match.Length);

            var unparsedTable = "<table " + html.Substring(match.Index, end - match.Index).Replace("<br>", " ").Replace("\n", " ") + "</table>";

            var items = XElement.Parse(unparsedTable).XPathSelectElements("//td");

            ParsedWords.Children.Clear();

            foreach (var word in items.SelectMany(x => x.Value.Split(' ')))
            {
                var f = FilterOut(word.ToLowerInvariant());

                if (IsAllowed(f) == false)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(f))
                {
                    continue;
                }

                ParsedWords.Children.Add(
                    new TextBlock()
                    {
                        Text = f
                    }
                );
            }

            bool IsAllowed(string input)
            {
                var notAllowed = new string[]
                {
                    "ОнОнаОно",
                    "Им",
                    "Р",
                    "Д",
                    "В",
                    "Тв",
                    "Пр",
                    "Вы",
                    "Мы",
                    "Ты",
                    "Я",
                    "Он",
                    "Она",
                    "Оно",
                    "Они",
                    "Пр",
                    "действ",
                    "наст",
                    "Пр",
                    "действ",
                    "прош",
                    "Деепр",
                    "наст",
                    "Деепр",
                    "про",
                    "Пр",
                    "страд",
                    "наст",
                    "Пр",
                    "страд",
                    "прош",
                    "Будущее",
                    "будубудешь",
                };

                foreach (var s in notAllowed)
                {
                    if (input == s.ToLowerInvariant())
                    {
                        return false;
                    }
                }

                return true;
            }

            string FilterOut(string input)
            {
                var r = "";
                var allowed = new char[]
                {
                    'а',
                    'б',
                    'в',
                    'г',
                    'д',
                    'е',
                    'ё',
                    'ж',
                    'з',
                    'и',
                    'й',
                    'к',
                    'л',
                    'м',
                    'н',
                    'о',
                    'п',
                    'р',
                    'с',
                    'т',
                    'у',
                    'ф',
                    'х',
                    'ц',
                    'ч',
                    'ш',
                    'щ',
                    'ъ',
                    'ы',
                    'ь',
                    'э',
                    'ю',
                    'я',
                };
                foreach (var l in input)
                {
                    if (allowed.Contains(l))
                    {
                        r += l;
                    }
                }

                return r;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            WordFromWiki.Navigate("https://ru.m.wiktionary.org/w/index.php?search=" + AdjustedNameForSearch.Text);
        }

        private void OpenSearch_OnClick(object sender, RoutedEventArgs e)
        {
            WordFromWiki.Navigate("https://ru.m.wiktionary.org/w/index.php?search=");
        }

        private async void Learn(object sender, RoutedEventArgs e)
        {
            Title = "Learning...";

            await BooksApplication.Instance.Dictionary.Learn(AdjustedWordToLearn.Text);

            Close();
        }

        private void AdjustedNameForSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AdjustedWordToLearn.Text = AdjustedNameForSearch.Text;
        }

        private void OpenGoogle_OnClick(object sender, RoutedEventArgs e)
        {
            WordFromWiki.Navigate("https://www.google.com/search?q=" + AdjustedNameForSearch.Text);
        }

        private async void LearnAll(object sender, RoutedEventArgs e)
        {
            Title = "Learning...";

            if (ParsedWords.Children.Count > 0)
            {
                foreach (TextBlock parsedWordsChild in ParsedWords.Children)
                {
                    await BooksApplication.Instance.Dictionary.Learn(parsedWordsChild.Text);
                }
            }

            Close();
        }
    }
}
