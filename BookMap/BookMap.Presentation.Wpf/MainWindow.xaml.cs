using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels;
using BookMap.Presentation.Wpf.Models;
using BookMap.Presentation.Wpf.Views;
using Newtonsoft.Json;
using MapInfo = BookMap.Presentation.Apple.Services.MapInfo;
using Measure = BookMap.Presentation.Wpf.InteractionModels.Measure;
using Path = System.IO.Path;

namespace BookMap.Presentation.Wpf
{
    //todo:
    // useful api to draw on image
    // pointer for moving, pointer for drawing, pointer for labels, they should be different
    //  pointer should correspond drawing area
    //  should have borders of colors of drawing or semitransparent
    //  ground or label on it with the color of drawing
    // palette, try to use wpf one, but not necessary
    // ability to create and fast configure new map

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Map.Init(this, mapName: "1");
        }
    }
}
