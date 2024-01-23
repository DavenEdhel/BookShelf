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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Map.Init(this, mapName: "1");

            Map.Pins.Visible = false;
        }
    }
}
