using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class ContentHolderView : View
    {
        private readonly Func<object, UIView> _createContent;

        private UIView _current;

        private readonly Dictionary<object, UIView> _reuseCache = new Dictionary<object, UIView>();

        public ContentHolderView(Func<object, UIView> createContent)
        {
            _createContent = createContent;   

            Initialize();
        }

        private void Initialize()
        {
            Layout = () =>
            {
                if (_current != null)
                {
                    _current.ChangeSize(Superview.Frame.Width - 200, Superview.Frame.Height);
                    _current.PositionToCenterHorizontally(this);

                    _current.LayoutSubviews();
                }
            };

            Layout();
        }

        public void Render(object data)
        {
            if (_reuseCache.ContainsKey(data))
            {
                Show(_reuseCache[data]);
                return;
            }

            var view = _createContent(data);

            _reuseCache.Add(data, view);

            Show(view);
        }

        private void Show(UIView view)
        {
            if (_current != null && _current is IContentView stateContainer)
            {
                stateContainer.Hide();
            }

            _current?.RemoveFromSuperview();

            _current = view;

            Add(view);

            Layout();

            if (view is IContentView stateContainerNewView)
            {
                stateContainerNewView.Show();
            }
        }
    }
}