using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

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

        private void Learn(object sender, RoutedEventArgs e)
        {
            BookShelf.Instance.Dictionary.Learn(AdjustedWordToLearn.Text);

            Close();
        }

        private void AdjustedNameForSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AdjustedWordToLearn.Text = AdjustedNameForSearch.Text;
        }
    }
}
