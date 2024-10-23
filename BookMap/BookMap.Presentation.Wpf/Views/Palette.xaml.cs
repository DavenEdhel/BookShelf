using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.Core;
using BookMap.Presentation.Wpf.InteractionModels;
using BookWiki.Core.Utils;
using static System.String;

namespace BookMap.Presentation.Wpf.Views
{
    public class Palettes
    {
        private readonly Canvas _container;
        private Dictionary<string, Palette> _all = new();
        private string _currentFeature;

        public Palettes(Canvas container)
        {
            _container = container;
        }

        public void Register(string feature, Palette palette)
        {
            _all[feature] = palette;
            palette.Visibility = Visibility.Collapsed;
            _container.Children.Add(palette);
        }

        public void Activate(string featureName)
        {
            _currentFeature = featureName;
        }

        public Palette Active
        {
            get
            {
                if (_currentFeature == null)
                {
                    return null;
                }

                if (_all.ContainsKey(_currentFeature))
                {
                    return _all[_currentFeature];
                }

                return null;
            }
        }
    }

    public interface IBrushes : IObservable<Unit>
    {
        BrushInfo[] Value { get; set; }

        int Current { get; set; }
    }

    public class BrushesFromMapSettingsPalettes : IBrushes
    {
        private readonly string _scope;
        private readonly MapProviderSynchronous _mapProvider;
        private readonly Subject<Unit> _changed = new();

        public BrushesFromMapSettingsPalettes(string scope, MapProviderSynchronous mapProvider)
        {
            _scope = scope;
            _mapProvider = mapProvider;
            _mapProvider.MapChanged += MapProviderOnMapChanged;
        }

        private void MapProviderOnMapChanged(string obj)
        {
            _changed.OnNext(Unit.Default);
        }

        public BrushInfo[] Value
        {
            get
            {
                var allPalettes = _mapProvider.Settings.PalettesV2;

                if (allPalettes.ContainsKey(_scope))
                {
                    return allPalettes[_scope].Brushes;
                }

                if (_mapProvider.Settings.Brushes != null)
                {
                    return _mapProvider.Settings.Brushes;
                }

                return Array.Empty<BrushInfo>();
            }
            set
            {
                _mapProvider.ChangeSettings(x => x.PalettesV2[_scope] = new PaletteDto()
                {
                    Brushes = value,
                    Selected = Current
                });
            }
        }

        public int Current
        {
            get
            {
                var allPalettes = _mapProvider.Settings.PalettesV2;

                if (allPalettes.ContainsKey(_scope))
                {
                    return allPalettes[_scope].Selected;
                }

                return 0;
            }
            set
            {
                _mapProvider.ChangeSettings(x => x.PalettesV2[_scope] = new PaletteDto()
                {
                    Brushes = Value,
                    Selected = value
                });
            }
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            return _changed.Subscribe(observer);
        }
    }


    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class Palette : UserControl
    {
        private readonly CurrentBrush _brush;
        private readonly IBrushes _brushes;
        private readonly CurrentBrush _cursorBrush;
        private readonly LabeledCursor _cursor;
        private readonly Scaling _scaling = new();
        private readonly Inverted _invertedScaling;

        public Palette()
        {
            InitializeComponent();
        }

        public Palette(CurrentBrush brush, CoordinateSystem coordinateSystem, IBrushes brushes)
        {
            _brush = brush;
            _brushes = brushes;
            _cursorBrush = new CurrentBrush();
            _cursorBrush.Set(_brush.Snapshot());
            InitializeComponent();

            foreach (var brushView in Brushes)
            {
                brushView.BrushTitle = string.Empty;
            }

            brushes.Subscribe(_ => MapChanged());

            B2Erase.Brush = new EraserBrush();
            B2Erase.BrushTitle = "Erase";

            _cursor = new LabeledCursor(CursorContainer, _cursorBrush, coordinateSystem);
            _cursor.Title = Empty;

            IsVisibleChanged += OnIsVisibleChanged;
            
            _cursor.PositionAndResizeAndColorize(new Point(92/2, 92/2));

            B3.Select();

            _cursorBrush.Subscribe(
                x =>
                {
                    InfoSize.Text = x.SizeInPixels.ToString();

                    var rgba = new RgbaColorFromBgra(_cursorBrush.Color.Bgra);
                    var hsva = new AhsvColorFromRgba(rgba);
                    InfoA.Text = hsva.A.ToString();
                    InfoH.Text = ((int)hsva.H).ToString();
                    InfoS.Text = ((int)(hsva.S*100)).ToString();
                    InfoV.Text = ((int)(hsva.V*100)).ToString();

                    SetPointBackground();
                    SetBackgrounds();

                    Brushes.First(x => x.IsSelected).Brush = x.Snapshot();
                }
            );

            _invertedScaling = new Inverted(_scaling, (int) Size.Minimum, (int) Size.Maximum);

            SetSliders();

            foreach (var brushView in Brushes)
            {
                brushView.Selected.Subscribe(
                    x =>
                    {
                        foreach (var view in Brushes)
                        {
                            view.Deselect();
                        }

                        x.Select();
                        _cursorBrush.Set(x.Brush);
                        SetSliders();
                    }
                );
            }
        }

        private void MapChanged()
        {
            int index = 0;
            foreach (var brushView in Brushes.Skip(2))
            {
                var savedBrush = _brushes.Value.ElementAtOrDefault(index);

                if (savedBrush == null)
                {
                    break;
                }

                brushView.Brush = new BrushFromConfig(savedBrush);
                index++;
            }

            var brushToSelect = Brushes.ElementAt(_brushes.Current + 2);
            _brush.Set(brushToSelect.Brush.Snapshot());
            brushToSelect.MakeSelected();
            _cursorBrush.Set(_brush.Snapshot());
            SetSliders();
        }

        private IEnumerable<BrushView> Brushes
        {
            get
            {
                yield return B1Last;
                yield return B2Erase;
                yield return B3;
                yield return B4;
                yield return B5;
                yield return B6;
                yield return B7;
                yield return B8;
                yield return B9;
                yield return B10;
                yield return B11;
                yield return B12;
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.CastTo<bool>())
            {
                B1Last.Brush = _brush.Snapshot();
                _cursorBrush.Set(_brush.Snapshot());
            }
            else
            {
                _brush.Set(_cursorBrush.Snapshot());

                var info = Brushes.Skip(2).Select(
                    x => new BrushInfo()
                    {
                        Size = x.Brush.SizeInPixels,
                        Color = new HexColorFromBgra(x.Brush.Color).Hex
                    }
                ).ToArray();

                _brushes.Value = info;
                _brushes.Current = Brushes.IndexOf(y => y == Brushes.First(x => x.IsSelected)) - 2;
            }
        }

        private void SizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _cursorBrush.SizeInPixels = _scaling.Value((int) e.NewValue);
        }

        private void SetSliders()
        {
            Size.Value = _invertedScaling.Value(_cursorBrush.SizeInPixels);

            var rgba = new RgbaColorFromBgra(_cursorBrush.Color.Bgra);
            var hsva = new AhsvColorFromRgba(rgba);
            H.Value = hsva.H;
            S.Value = hsva.S*100;
            V.Value = hsva.V*100;
            A.Value = hsva.A;
        }

        private void SetBackgrounds()
        {
            var rgba = new RgbaColorFromBgra(_cursorBrush.Color.Bgra);
            var hsva = new AhsvColorFromRgba(rgba);

            H.Background = new LinearGradientBrush(
                new GradientStopCollection(
                    new GradientStop[]
                    {
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 0)
                                )
                            ).Color,
                            0
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 1)
                                )
                            ).Color,
                            60f * 1f / 360f
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 2)
                                )
                            ).Color,
                            60f * 2f / 360f
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 3)
                                )
                            ).Color,
                            60f * 3f / 360f
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 4)
                                )
                            ).Color,
                            60f * 4f / 360f
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 5)
                                )
                            ).Color,
                            60f * 5f / 360f
                        ),
                        new GradientStop(
                            new MediaColorFromArgbColor(
                                new ArgbColorFromAhsv(
                                    new ModifiedAhsv(hsva, h: 60 * 6)
                                )
                            ).Color,
                            60f * 6f / 360f
                        ),
                    }
                ),
                new Point(0, 0),
                new Point(1, 0)
            );

            S.Background = new LinearGradientBrush(
                new MediaColorFromArgbColor(
                    new ArgbColorFromAhsv(
                        new ModifiedAhsv(hsva, s: 0)
                    )
                ).Color,
                new MediaColorFromArgbColor(
                    new ArgbColorFromAhsv(
                        new ModifiedAhsv(hsva, s: 1)
                    )
                ).Color,
                0
            );

            V.Background = new LinearGradientBrush(
                new MediaColorFromArgbColor(
                    new ArgbColorFromAhsv(
                        new ModifiedAhsv(hsva, v: 0)
                    )
                ).Color,
                new MediaColorFromArgbColor(
                    new ArgbColorFromAhsv(
                        new ModifiedAhsv(hsva, v: 1)
                    )
                ).Color,
                0
            );
        }

        private void H_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _cursorBrush.Color = new BgraColorFromArgb(
                new ArgbColorFromAhsv(
                    new ModifiedAhsv(
                        new AhsvColorFromRgba(
                            new RgbaColorFromBgra(_cursorBrush.Color.Bgra)
                        ),
                        h: (float) H.Value
                    )
                )
            );
        }

        private void S_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _cursorBrush.Color = new BgraColorFromArgb(
                new ArgbColorFromAhsv(
                    new ModifiedAhsv(
                        new AhsvColorFromRgba(
                            new RgbaColorFromBgra(_cursorBrush.Color.Bgra)
                        ),
                        s: (float)S.Value/100f
                    )
                )
            );
        }

        private void V_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _cursorBrush.Color = new BgraColorFromArgb(
                new ArgbColorFromAhsv(
                    new ModifiedAhsv(
                        new AhsvColorFromRgba(
                            new RgbaColorFromBgra(_cursorBrush.Color.Bgra)
                        ),
                        v: (float)V.Value / 100f
                    )
                )
            );
        }

        private void A_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _cursorBrush.Color = new ModifiedBgra(_cursorBrush.Color.Bgra, a: (byte)A.Value);
        }

        private void SetPointBackground()
        {
            var color = new MediaColorFromArgbColor(
                new RgbWithAlpha(
                    new ComplementaryArgb(
                        new RgbaColorFromBgra(
                            _cursorBrush.Color.Bgra
                        )
                    ),
                    16
                )
            );

            CursorContainer.Background = new SolidColorBrush(color.Color);
        }
    }

    public interface IFunction<TIn, TOut>
    {
        TOut Value(TIn x);
    }

    public class Inverted : IFunction<int, int>
    {
        private readonly IFunction<int, int> _current;
        private readonly int _min;
        private readonly int _max;

        public Inverted(IFunction<int, int> current, int min, int max)
        {
            _current = current;
            _min = min;
            _max = max;
        }

        public int Value(int x)
        {
            var left = _min;
            var right = _max;
            var iterationsLeft = 500;

            while (true)
            {
                var current = (left + right) / 2;

                var currentValue = _current.Value(current);

                if (currentValue == x)
                {
                    return current;
                }

                if (x > currentValue)
                {
                    left = current;
                }
                else
                {
                    right = current;
                }

                iterationsLeft--;

                if (iterationsLeft < 0)
                {
                    return current;
                }
            }
        }
    }

    public class Scaling : IFunction<int, int>
    {
        public int Value(int x)
        {
            var leftBorder = 2f;
            var rightBorder = 6f;
            var total = 153f;
            var min = 1;
            var basement = 4f;

            var result = leftBorder + (x * (rightBorder - leftBorder) / total);
            var result2 = (Math.Pow(basement, result) - Math.Pow(basement, leftBorder))/Math.Pow(basement, rightBorder);
            var result3 = result2 * (total - min);
            var result4 = (int) result3 + min;
            return result4;
        }
    }
}
