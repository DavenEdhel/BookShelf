using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
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
using BookMap.Presentation.Wpf.InteractionModels;

namespace BookMap.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for BrushView.xaml
    /// </summary>
    public partial class BrushView : UserControl
    {
        private IBrush _brush;
        private readonly Subject<BrushView> _selected = new Subject<BrushView>();

        public BrushView()
        {
            InitializeComponent();

            Brush = new DefaultBrush();
        }

        public IObservable<BrushView> Selected => _selected;

        public bool IsSelected => Border.Visibility == Visibility.Visible;

        private void BrushClicked(object sender, MouseButtonEventArgs e)
        {
            _selected.OnNext(this);
        }

        public void Select()
        {
            Border.Visibility = Visibility.Visible;
        }

        public void Deselect()
        {
            Border.Visibility = Visibility.Hidden;
        }

        public IBrush Brush
        {
            get => _brush;
            set
            {
                _brush = value;

                Size.Text = _brush.SizeInPixels.ToString();
                Color.Background = new SolidColorBrush(
                    new MediaColorFromArgbColor(
                        new RgbaColorFromBgra(_brush.Color.Bgra)
                    ).Color
                );
            }
        }

        public string BrushTitle
        {
            set
            {
                Title.Text = value;
            }
        }
    }
}
