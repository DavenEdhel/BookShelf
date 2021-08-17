using System;
using BookMap.Presentation.Apple.Services;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class ActionBarView : UIView
    {
        private readonly MapProvider _mapProvider;
        private UISegmentedControl _drawModeSegment;
        private UIButton _showLabels;
        private UIButton _togglePalette;
        private UIButton _measure;

        public event Action OpenMenu = delegate { };

        public event Action<TouchMode> TouchModeChanged = delegate { };

        public event Action<DrawMode> DrawModeChanged = delegate { };

        public event Action<bool> ShowLabelsChanged = delegate { };

        public event Action ImportClicked = delegate { };

        public event Action DrawUpClicked = delegate { };

        public event Action<bool> TogglePaletteClicked = delegate { };

        public event Action<bool> ToggleBookmarksClicked = delegate { };

        public event Action MakeMeasureClicked = delegate { };

        public bool IsPaletteVisible { get; private set; } = true;

        public bool IsBookmarksVisible { get; private set; } = true;

        public TouchMode TouchMode => GetTouchMode("Unlock", _measure.TitleLabel.Text);

        public DrawMode DrawMode
        {
            get => _drawModeSegment.SelectedSegment == 0 ? DrawMode.Ground : DrawMode.Labels;
            set => _drawModeSegment.SelectedSegment = value == DrawMode.Ground ? 0 : 1;
        } 

        public bool ShowLabels => _showLabels.TitleLabel.Text == "Hide labels";

        public ActionBarView(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Gray.ColorWithAlpha(0.3f);

            var reset = new UIButton(UIButtonType.RoundedRect);
            reset.SetTitleColor(UIColor.Red, UIControlState.Normal);
            reset.SetTitle("Menu", UIControlState.Normal);
            reset.Frame = new CGRect(0, 0, 100, 50);
            reset.TouchUpInside += ResetOnTouchUpInside;
            Add(reset);

            _measure = new UIButton(UIButtonType.RoundedRect);
            _measure.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _measure.SetTitle("Measure", UIControlState.Normal);
            _measure.TouchUpInside += MeasureOnValueChanged;
            Add(_measure);

            _drawModeSegment = new UISegmentedControl();
            _drawModeSegment.InsertSegment("ground", 0, false);
            _drawModeSegment.InsertSegment("labels", 1, false);
            _drawModeSegment.SelectedSegment = 1;
            _drawModeSegment.ValueChanged += DrawModeSegmentOnValueChanged;
            Add(_drawModeSegment);

            _showLabels = new UIButton();
            _showLabels.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _showLabels.SetTitle("Hide labels", UIControlState.Normal);
            _showLabels.TouchUpInside += ShowLabelsOnTouchUpInside;
            Add(_showLabels);

            var import = new UIButton(UIButtonType.RoundedRect);
            import.SetTitleColor(UIColor.Red, UIControlState.Normal);
            import.SetTitle("Import", UIControlState.Normal);
            import.Frame = new CGRect(0, 0, 100, 50);
            import.TouchUpInside += ImportOnTouchUpInside;
            Add(import);

            var drawUp = new UIButton(UIButtonType.RoundedRect);
            drawUp.SetTitleColor(UIColor.Red, UIControlState.Normal);
            drawUp.SetTitle("Draw Up", UIControlState.Normal);
            drawUp.Frame = new CGRect(0, 0, 100, 50);
            drawUp.TouchUpInside += OkOnTouchUpInside;
            Add(drawUp);

            _togglePalette = new UIButton(UIButtonType.RoundedRect);
            _togglePalette.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _togglePalette.TouchUpInside += PaletteToggleOnTouchUpInside;
            UpdateToggleTitle();
            Add(_togglePalette);

            var makeMeasure = new UIButton(UIButtonType.RoundedRect);
            makeMeasure.SetTitleColor(UIColor.Red, UIControlState.Normal);
            makeMeasure.SetTitle("Make measure", UIControlState.Normal);
            makeMeasure.TouchUpInside += SettingsOnTouchUpInside;
            Add(makeMeasure);

            _toggleBookmarks = new UIButton(UIButtonType.RoundedRect);
            _toggleBookmarks.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _toggleBookmarks.TouchUpInside += ToggleBookmarksOnTouchUpInside;
            UpdateBookmarkTitle();
            Add(_toggleBookmarks);

            ScaleLabel = new UILabel();
            ScaleLabel.Font = UIFont.BoldSystemFontOfSize(18);
            ScaleLabel.TextColor = UIColor.Black;
            Add(ScaleLabel);

            this.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            this.AddConstraints(
                reset.AtLeftOf(this, 10),
                reset.CenterY().EqualTo().CenterYOf(this),

                _measure.ToRightOf(reset, 10),
                _measure.CenterY().EqualTo().CenterYOf(this),
                _measure.Width().EqualTo(100),
                _measure.Height().EqualTo(30),

                _drawModeSegment.ToRightOf(_measure, 10),
                _drawModeSegment.CenterY().EqualTo().CenterYOf(this),
                _drawModeSegment.Width().EqualTo(200),
                _drawModeSegment.Height().EqualTo(30),
                
                _showLabels.ToRightOf(_drawModeSegment, 3),
                _showLabels.Height().EqualTo(30),
                _showLabels.Width().EqualTo(100),
                _showLabels.CenterY().EqualTo().CenterYOf(this),

                import.ToRightOf(_showLabels, 10),
                import.CenterY().EqualTo().CenterYOf(this),

                drawUp.ToRightOf(import, 10),
                drawUp.CenterY().EqualTo().CenterYOf(this),

                _togglePalette.ToRightOf(drawUp, 10),
                _togglePalette.CenterY().EqualTo().CenterYOf(this),

                makeMeasure.ToRightOf(_togglePalette, 10),
                makeMeasure.CenterY().EqualTo().CenterYOf(this),

                _toggleBookmarks.ToRightOf(makeMeasure, 10),
                _toggleBookmarks.CenterY().EqualTo().CenterYOf(this),

                ScaleLabel.ToRightOf(_toggleBookmarks, 10),
                ScaleLabel.CenterY().EqualTo().CenterYOf(this));
        }

        private void ToggleBookmarksOnTouchUpInside(object sender, EventArgs e)
        {
            IsBookmarksVisible = !IsBookmarksVisible;

            UpdateBookmarkTitle();

            ToggleBookmarksClicked(IsBookmarksVisible);

            _mapProvider.ChangeSettings(s => s.ShowBookmarks = IsBookmarksVisible);
        }

        public UILabel ScaleLabel { get; set; }

        private void MeasureOnValueChanged(object sender, EventArgs e)
        {
            var measure = _measure.TitleLabel.Text == "Measure" ? "Draw" : "Measure";

            _measure.SetTitle(measure, UIControlState.Normal);

            TouchModeChanged(GetTouchMode("Lock", measure));
        }

        private void ShowLabelsOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            ToggleShowLabels();
        }

        private DrawMode? _lastValue = null;
        private UIButton _toggleBookmarks;

        private void ToggleShowLabels()
        {
            if (_showLabels.TitleLabel.Text == "Hide labels")
            {
                _showLabels.SetTitle("Show labels", UIControlState.Normal);
            }
            else
            {
                _showLabels.SetTitle("Hide labels", UIControlState.Normal);

                if (_lastValue != null)
                {
                    ChangeDrawModeFromCode(_lastValue.Value);
                    _lastValue = null;
                }
            }

            if (ShowLabels == false && DrawMode == DrawMode.Labels)
            {
                ChangeDrawModeFromCode(DrawMode.Ground);
                _lastValue = DrawMode.Labels;
            }

            ShowLabelsChanged(ShowLabels);
        }

        private void ChangeDrawModeFromCode(DrawMode value)
        {
            _drawModeSegment.ValueChanged -= DrawModeSegmentOnValueChanged;
            DrawMode = value;
            DrawModeChanged(DrawMode);
            _drawModeSegment.ValueChanged += DrawModeSegmentOnValueChanged;
        }

        private void SettingsOnTouchUpInside(object sender, EventArgs e)
        {
            MakeMeasureClicked();
        }

        private void PaletteToggleOnTouchUpInside(object sender, EventArgs e)
        {
            IsPaletteVisible = !IsPaletteVisible;

            UpdateToggleTitle();

            TogglePaletteClicked(IsPaletteVisible);

            _mapProvider.ChangeSettings(s => s.ShowPalette = IsPaletteVisible);
        }

        public void SetPaletteVisibility(bool isVisible)
        {
            if (IsPaletteVisible == isVisible)
            {
                return;
            }

            PaletteToggleOnTouchUpInside(this, EventArgs.Empty);
        }

        public void SetBookmarksVisibility(bool isVisible)
        {
            if (IsBookmarksVisible == isVisible)
            {
                return;
            }

            ToggleBookmarksOnTouchUpInside(this, EventArgs.Empty);
        }

        private void UpdateToggleTitle()
        {
            _togglePalette.SetTitle(IsPaletteVisible ? "Hide Palette" : "Show Palette", UIControlState.Normal);
        }

        private void UpdateBookmarkTitle()
        {
            _toggleBookmarks.SetTitle(IsBookmarksVisible ? "Hide Bookmarks" : "Show Bookmarks", UIControlState.Normal);
        }

        private void OkOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            DrawUpClicked();
        }

        private void ImportOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            ImportClicked();
        }
        
        private void DrawModeSegmentOnValueChanged(object sender, EventArgs e)
        {
            _lastValue = null;

            DrawModeChanged(DrawMode);

            if (ShowLabels == false && DrawMode == DrawMode.Labels)
            {
                ToggleShowLabels();
            }
        }

        private TouchMode GetTouchMode(string moveAndLockText, string measureText)
        {
            if (measureText == "Draw")
            {
                return TouchMode.MoveAndMeasure;
            }
            else if (moveAndLockText == "Lock")
            {
                return TouchMode.MoveAndDraw;
            }
            else
            {
                return TouchMode.DrawOnly;
            }
        }

        private void ResetOnTouchUpInside(object sender, EventArgs e)
        {
            OpenMenu();
        }

        public void StopMeasuring()
        {
            if (TouchMode == TouchMode.MoveAndMeasure)
            {
                _measure.SetTitle("Measure", UIControlState.Normal);

                TouchModeChanged(GetTouchMode("Lock", "Measure"));
            }
        }
    }
}