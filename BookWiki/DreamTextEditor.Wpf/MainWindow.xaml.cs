using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DreamTextEditor.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            if (e.Key == Key.Enter)
            {
                AddNewItem();
            }
        }

        private void AddNewItem()
        {
            var tb = new TextBlock();
            tb.Text = "123";

            TextPanel.Children.Add(tb);
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            AddNewItem();
        }

        private void InsertItem(object sender, RoutedEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            var tb = new TextBlock();
            tb.Text = "123";

            TextPanelFirst.Children.Insert(0, tb);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            TextPanel.Children.Clear();
        }

        private void RenderALot(object sender, RoutedEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            for (int i = 0; i < 10000; i++)
            {
                AddNewItem();
            }
        }

        private void InsertItem2(object sender, RoutedEventArgs e)
        {
            using var monitor = new Monitor(LastOperationTime);

            var tb = new TextBlock();
            tb.Text = "123";

            TextPanel.Children.Insert(0, tb);
        }
    }

    public class Monitor : IDisposable
    {
        private readonly TextBlock _output;
        private readonly Stopwatch _sw;

        public Monitor(TextBlock output)
        {
            _output = output;
            _sw = new Stopwatch();
            _sw.Start();
        }

        public void Dispose()
        {
            _output.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() =>
                {
                    _sw.Stop();
                    _output.Text = _sw.Elapsed.TotalMilliseconds.ToString("N");
                }));
        }
    }
}