using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnActivated(EventArgs e)
        {
            BooksApplication.Instance.RestoreLastSession();

            base.OnActivated(e);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

            File.AppendAllText("Logs.txt", $"\n\nStart session {DateTime.Now.ToString("U")}\n");
        }

        private void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            File.AppendAllText("Logs.txt", "CurrentDomainOnFirstChanceException\n");
            File.AppendAllText("Logs.txt", e.Exception.ToString() + "\n\n");
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.AppendAllText("Logs.txt", "CurrentDomainOnUnhandledException\n");
            File.AppendAllText("Logs.txt", e.ExceptionObject.ToString() + "\n\n");
        }
    }
}
