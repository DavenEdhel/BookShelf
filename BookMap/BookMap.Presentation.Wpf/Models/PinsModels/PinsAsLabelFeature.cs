using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookMap.Presentation.Apple.Services;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookMap.Presentation.Wpf.Models
{
    public class PinsAsLabelFeature : IPinsFeature
    {
        public bool CanCreateNew => true;

        public PinInfo NewPin()
        {
            return new PinInfo()
            {
                Name = "Some Active Pin"
            };
        }

        public UIElement CreateView(PinDto info)
        {
            return new TextBox()
            {
                Text = info.Name,
                Tag = info.Id,
                Background = new SolidColorBrush(Colors.Red)
            };
        }

        public void SetNormal(UIElement view)
        {
            view.CastTo<TextBox>().Background = new SolidColorBrush(Colors.Red);
        }

        public void SetHighlighted(UIElement view)
        {
            view.CastTo<TextBox>().Background = new SolidColorBrush(Colors.Blue);
        }
    }
}