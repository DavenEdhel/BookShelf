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

namespace BookMap.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for Experiments.xaml
    /// </summary>
    public partial class Experiments : Window
    {
        private int w = 2560;
        private int h = 1920;

        public Experiments()
        {
            InitializeComponent();

            var i = new BitmapImage(
                new Uri("C:\\Work\\Projects\\BookShelf\\BookMap\\BookMap.Presentation.Wpf\\1\\ground_0_0_0.png")
            );

            Container.MouseDown += ContainerOnMouseDown;
            Container.MouseUp += ContainerOnMouseUp;
            Container.MouseMove += ContainerOnMouseMove;

            _editor = new WriteableBitmap(i);

            Img.Source = _editor;
        }

        private bool _moveInProgress;
        private Point _initialPoint = new Point(0, 0);
        private Vector _offset = new Vector(0, 0);
        private Point _zoomLocation = new Point(0, 0);
        private bool _measureMode;
        private readonly WriteableBitmap _editor;

        private void ContainerOnMouseMove(object sender, MouseEventArgs e)
        {
            var current = e.GetPosition(Container);

            Title = $"{current.X} {current.Y}";

            _offset = _initialPoint - current;

            var area = new Int32Rect((int)current.X, (int)current.Y, 10, 10);
            var part = new uint[10 * 10];
            var stride = 10 * 4;
            var offset = 0;

            _editor.CopyPixels(area, part, stride, offset);

            for (var i = 0; i < 20; i++)
            {
                part[i] = 0;
            }

            _editor.WritePixels(area, part, stride, offset);
        }

        private void ContainerOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var current = e.GetPosition(Container);

            Title = $"{current.X} {current.Y}";
        }

        private void ContainerOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var current = e.GetPosition(Container);

            Title = $"{current.X} {current.Y}";
        }
    }
}
