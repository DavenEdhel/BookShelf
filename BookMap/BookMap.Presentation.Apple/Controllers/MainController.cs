using System;
using System.Drawing;
using System.IO;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Apple.Views;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Controllers
{
    public class MainController : UIViewController
    {
        private ActionBarView _topPanel;
        private MapView _mapView;
        private SingleImageView _importView;
        private PaletteView _palettePanel;

        private CoordinateSystem _coordinateSystem;
        private BrushEditorView _brushEditView;
        private MenuView _menuView;
        private MapProvider _mapProvider;
        private MakeMeasureWizardView _makeMeasureWizardView;
        private UIImagePickerController _pickerController;
        private BookmarksView _bookmarksView;
        private Session _session;

        public override void ViewDidLoad()
        {
            _mapProvider = new MapProvider(new ImageFactory());
            _mapProvider.MapChanged += MapProviderOnMapChanged;

            _coordinateSystem = new CoordinateSystem();
            _coordinateSystem.LevelScaleChanged += LevelScaleChanged;

            UIApplication.SharedApplication.SetStatusBarHidden(true, false);

            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            _topPanel = new ActionBarView(_mapProvider);
            _topPanel.OpenMenu += TopPanelOnOpenMenu;
            _topPanel.TouchModeChanged += TopPanelOnTouchModeChanged;
            _topPanel.DrawModeChanged += TopPanelOnDrawModeChanged;
            _topPanel.ShowLabelsChanged += TopPanelOnShowLabelsChanged;
            _topPanel.ImportClicked += TopPanelOnImportClicked;
            _topPanel.TogglePaletteClicked += TopPanelOnTogglePaletteClicked;
            _topPanel.MakeMeasureClicked += TopPanelOnMakeMeasureClicked;
            _topPanel.DrawUpClicked += TopPanelOnDrawUpClicked;
            _topPanel.ToggleBookmarksClicked += TopPanelOnToggleBookmarksClicked;
            View.Add(_topPanel);

            _palettePanel = new PaletteView(_mapProvider);
            _palettePanel.EditBrush += PalettePanelOnEditBrush;
            
            _mapView = new MapView(_coordinateSystem, _palettePanel, _mapProvider, _topPanel);
            _mapView.Frame = View.Frame;
            _mapView.ShowLabels = _topPanel.ShowLabels;
            _mapView.DrawMode = _topPanel.DrawMode;
            _mapView.ColorSelected += MapViewOnColorSelected;
            View.Add(_mapView);

            _brushEditView = new BrushEditorView(_mapView, _palettePanel);

            _menuView = new MenuView(_mapView);

            _bookmarksView = new BookmarksView(_mapView, _mapProvider);
            Add(_bookmarksView);

            _importView = new SingleImageView();
            _importView.Frame = View.Frame;
            _importView.OkClicked += TopPanelOnOkClicked;
            _importView.CancelClicked += TopPanelOnCancelClicked;

            _makeMeasureWizardView = new MakeMeasureWizardView();
            _makeMeasureWizardView.MeasurePerformed += MakeMeasureWizardViewOnMeasurePerformed;

            View.BringSubviewToFront(_topPanel);

            UpdatePaletteVisibility(_topPanel.IsPaletteVisible);
            UpdateBookmarksVisibility(_topPanel.IsBookmarksVisible);

            _session = new Session();
            _session.Load();
            
            if (string.IsNullOrWhiteSpace(_session.Info.LastOpenedMap) == false)
            {
                try
                {
                    _mapView.Load(_session.Info.LastOpenedMap, _session.Info.LastOpenedLocation);

                    if (string.IsNullOrWhiteSpace(_session.Info.LastOpenedLocation) == false)
                    {
                        _bookmarksView.Select(_session.Info.LastOpenedLocation);
                    }
                }
                catch (Exception e)
                {
                
                }
            }
            else
            {
                Add(_menuView);
            }
        }

        public void StoreData()
        {
            _session.Info.LastOpenedMap = _mapProvider.CurrentMap;
            _session.Info.LastOpenedLocation = _bookmarksView.SelectedBookmark?.Name ?? string.Empty;

            _session.Save();
        }

        private void MapProviderOnMapChanged(string obj)
        {
            var settings = _mapProvider.Settings;

            _topPanel.SetPaletteVisibility(settings.ShowPalette);

            _topPanel.SetBookmarksVisibility(settings.ShowBookmarks);
        }

        private void UpdateBookmarksVisibility(bool isVisible)
        {
            if (isVisible && _bookmarksView.Superview == null)
            {
                View.Add(_bookmarksView);
            }

            if (isVisible == false && _bookmarksView.Superview != null)
            {
                _bookmarksView.RemoveFromSuperview();
            }
        }

        private void TopPanelOnToggleBookmarksClicked(bool obj)
        {
            UpdateBookmarksVisibility(obj);
        }

        private void TopPanelOnDrawUpClicked()
        {
            _mapView.DrawUp();
        }

        private void MakeMeasureWizardViewOnMeasurePerformed(double distance, double inMeters)
        {
            var scaled = distance / _coordinateSystem.Scale;
            var widthInMeters = inMeters / scaled * 1024;

            _mapProvider.ChangeSettings(settings =>
                {
                    settings.Width = widthInMeters;
                });

            _coordinateSystem.ActualWidthInMeters = widthInMeters;
        }

        private void TopPanelOnMakeMeasureClicked()
        {
            Add(_makeMeasureWizardView);
        }

        private void MapViewOnColorSelected(UIColor uiColor)
        {
            _brushEditView.SetColorFromSelector(uiColor);
        }

        private void PalettePanelOnEditBrush(BrushTile brushTile)
        {
            _brushEditView.Tile = brushTile;
            _mapView.ColorEditMode = true;

            View.Add(_brushEditView);
        }

        private void LevelScaleChanged(double scale)
        {
            _palettePanel.Scale = (float)scale;
            _topPanel.ScaleLabel.Text = $"{_coordinateSystem.Level:F}L";
        }

        public void TopPanelOnTogglePaletteClicked(bool isVisible)
        {
            UpdatePaletteVisibility(isVisible);
        }

        private void UpdatePaletteVisibility(bool isVisible)
        {
            if (isVisible && _palettePanel.Superview == null)
            {
                View.Add(_palettePanel);
            }

            if (isVisible == false && _palettePanel.Superview != null)
            {
                _palettePanel.RemoveFromSuperview();
            }
        }

        private void TopPanelOnCancelClicked()
        {
            _importView.RemoveFromSuperview();

            _mapView.UserInteractionEnabled = true;
        }

        private void TopPanelOnOkClicked()
        {
            _importView.RemoveFromSuperview();

            _mapView.UserInteractionEnabled = true;

            _mapView.ImportImage(_importView.FrameDouble, _importView.ImageToImport);
        }

        private void TopPanelOnImportClicked()
        {
            _mapView.UserInteractionEnabled = false;

            View.Add(_importView);

            View.BringSubviewToFront(_topPanel);

            if (UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.PhotoLibrary))
            {
                _pickerController = new UIImagePickerController();
                _pickerController.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
                _pickerController.FinishedPickingMedia += PickerControllerOnFinishedPickingMedia;
                _pickerController.Canceled += PickerControllerOnCanceled;
                _pickerController.ModalPresentationStyle = UIModalPresentationStyle.Popover;

                PresentViewController(_pickerController, animated: true, completionHandler: () => { });

                var popover = _pickerController.PopoverPresentationController;
                popover.SourceRect = _topPanel.Frame;
                popover.SourceView = _topPanel;
                popover.PermittedArrowDirections = UIPopoverArrowDirection.Any;
            }
        }

        private void PickerControllerOnFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs uiImagePickerMediaPickedEventArgs)
        {
            _pickerController.DismissViewController(true, () => { });

            _importView.Load(uiImagePickerMediaPickedEventArgs.OriginalImage);
        }

        private void PickerControllerOnCanceled(object sender, EventArgs eventArgs)
        {
            _pickerController.DismissViewController(true, () => { });
        }

        private void PickerControllerOnFinishedPickingImage(object sender, UIImagePickerImagePickedEventArgs uiImagePickerImagePickedEventArgs)
        {
            _pickerController.DismissViewController(true, () => { });

            _importView.Load(uiImagePickerImagePickedEventArgs.Image);
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        private void TopPanelOnShowLabelsChanged(bool showLabels)
        {
            _mapView.ShowLabels = showLabels;
        }

        private void TopPanelOnDrawModeChanged(DrawMode drawMode)
        {
            _mapView.DrawMode = drawMode;
        }

        private void TopPanelOnTouchModeChanged(TouchMode touchMode)
        {
            _mapView.TouchMode = touchMode;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _topPanel.Frame = new CGRect(0, 0, View.Frame.Width, 30);
            _palettePanel.Frame = new CGRect(0, 30, 75, View.Frame.Height);

            var dialogSize = new CGSize(View.Frame.Width / 2.7, View.Frame.Height / 2);
            var dialogPosition = new CGPoint(View.Frame.Width / 2 - dialogSize.Width / 2, View.Frame.Height / 2 - dialogSize.Height / 2);
            _brushEditView.Frame = new CGRect(dialogPosition, dialogSize);

            _menuView.Frame = new CGRect(dialogPosition, dialogSize);

            _makeMeasureWizardView.Frame = View.Frame;

            _bookmarksView.Frame = new CGRect(View.Frame.Width - 150, 30, 150, View.Frame.Height - 30);
        }

        private void TopPanelOnOpenMenu()
        {
            Add(_menuView);
        }
    }
}