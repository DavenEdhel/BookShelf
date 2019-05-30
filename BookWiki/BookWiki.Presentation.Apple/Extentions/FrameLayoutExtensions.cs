using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Extentions
{
    public static class FrameLayoutExtensions
    {
        public static nfloat RightInside(this UIView element, UIView another, float margin = 0)
        {
            return another.Frame.Width - element.Frame.Width - margin;
        }

        public static nfloat BottomInside(this UIView element, UIView another, float margin = 0)
        {
            var result = another.Frame.Height - element.Frame.Height - margin;

            return result;
        }

        public static nfloat LeftInside(this UIView element, UIView another, float margin = 0)
        {
            return margin;
        }

        public static nfloat CenterVertically(this UIView element, nfloat containerHeight)
        {
            return containerHeight / 2 - element.Frame.Height / 2;
        }

        public static nfloat CenterHorizontally(this UIView element, nfloat containerWidth)
        {
            return containerWidth / 2 - element.Frame.Width / 2;
        }

        public static nfloat CenterVerticallyInside(this UIView element, UIView another)
        {
            return another.Frame.Height / 2 - element.Frame.Height / 2;
        }

        public static nfloat CenterHorizontallyInside(this UIView element, UIView another)
        {
            return another.Frame.Left + another.Frame.Width / 2 - element.Frame.Width / 2;
        }

        public static void Position(this UIView element, nfloat x, nfloat y)
        {
            element.Frame = new CGRect(x, y, element.Frame.Width, element.Frame.Height);
        }

        public static void PositionWithTheSameOf(this UIView element, UIView container, float left = 0, float right = 0,
            float top = 0, float bottom = 0)
        {
            element.Frame = new CGRect(container.Frame.X - left, container.Frame.Y + top, container.Frame.Width - left - right, container.Frame.Height - top - bottom);
        }

        public static void PositionUnder(this UIView element, UIView another, float topMargin = 0)
        {
            element.ChangeY(another.Frame.Y + another.Frame.Height + topMargin);
        }

        public static void PositionAbove(this UIView element, UIView another, float topMargin = 0)
        {
            element.ChangeY(another.Frame.Y - another.Frame.Height - topMargin - element.Frame.Height);
        }

        public static void PositionToLeft(this UIView element, UIView another, float marginLeft = 0)
        {
            element.ChangeX(another.Frame.Left + marginLeft);
        }

        public static void PositionToRight(this UIView element, UIView another, float marginRight = 0)
        {
            element.ChangeX(another.Frame.Right - element.Frame.Width - marginRight);
        }

        public static void PositionAtRightOf(this UIView element, UIView another, float marginRight = 0)
        {
            element.ChangeX(another.Frame.Right + marginRight);
        }

        public static void PositionToCenterHorizontally(this UIView element, UIView another)
        {
            element.ChangeX(element.CenterHorizontallyInside(another));
        }

        public static void PositionToCenterVertically(this UIView element, UIView another)
        {
            element.ChangeY(element.CenterVerticallyInside(another));
        }

        public static void PositionToRightAndCenterInside(this UIView element, UIView another, float marginRight = 0)
        {
            element.Frame = new CGRect(element.RightInside(another, marginRight), element.CenterVerticallyInside(another), element.Frame.Width, element.Frame.Height);
        }

        public static void PositionToLeftAndCenterInside(this UIView element, UIView another, float leftMargin = 0)
        {
            element.Frame = new CGRect(element.LeftInside(another, leftMargin), element.CenterVerticallyInside(another), element.Frame.Width, element.Frame.Height);
        }

        public static void PositionToRightAndBottomInside(this UIView element, UIView another, float rightMargin = 0,
            float bottomMargin = 0)
        {
            element.Frame = new CGRect(element.RightInside(another, rightMargin), element.BottomInside(another, bottomMargin), element.Frame.Width, element.Frame.Height);
        }

        public static void PositionToCenterInside(this UIView element, UIView another)
        {
            element.Frame = new CGRect(element.CenterHorizontallyInside(another), element.CenterVerticallyInside(another), element.Frame.Width, element.Frame.Height);
        }

        public static void PositionToCenterInside(this UIView element, nfloat width, nfloat height)
        {
            element.Frame = new CGRect(element.CenterHorizontally(width), element.CenterVertically(height), element.Frame.Width, element.Frame.Height);
        }

        public static void ChangeHeight(this UIView element, nfloat height)
        {
            element.Frame = new CGRect(new CGPoint(element.Frame.X, element.Frame.Y), new CGSize(element.Frame.Width, height));
        }

        public static void ChangeWidth(this UIView element, nfloat width)
        {
            element.Frame = new CGRect(new CGPoint(element.Frame.X, element.Frame.Y), new CGSize(width, element.Frame.Height));
        }

        public static void SameWidthOf(this UIView element, UIView another, float margin = 0)
        {
            element.ChangeWidth(another.Frame.Width - 2 * margin);
        }

        public static void ChangeSize(this UIView element, nfloat width, nfloat height)
        {
            element.Frame = new CGRect(element.Frame.Location, new CGSize(width, height));
        }

        public static void SetSizeThatFits(this UIView element, nfloat? width = null, nfloat? height = null)
        {
            var size = element.SizeThatFits(new CGSize(width.GetValueOrDefault(float.MaxValue), height.GetValueOrDefault(float.MaxValue)));

            element.Frame = new CGRect(element.Frame.Location, new CGSize(width ?? size.Width, height ?? size.Height));
        }

        public static void ChangeY(this UIView element, nfloat y)
        {
            element.Frame = new CGRect(new CGPoint(element.Frame.X, y), new CGSize(element.Frame.Width, element.Frame.Height));
        }

        public static void ChangeX(this UIView element, nfloat x)
        {
            element.Frame = new CGRect(new CGPoint(x, element.Frame.Y), new CGSize(element.Frame.Width, element.Frame.Height));
        }

        public static void ChangePosition(this UIView element, nfloat x, nfloat y)
        {
            element.Frame = new CGRect(new CGPoint(x, y), new CGSize(element.Frame.Width, element.Frame.Height));
        }

        public static CGPoint Move(this CGPoint p, int dx = 0, int dy = 0)
        {
            return new CGPoint(p.X + dx, p.Y + dy);
        }
    }
}
