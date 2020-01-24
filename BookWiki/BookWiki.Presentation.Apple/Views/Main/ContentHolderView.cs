using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Controls;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class ContentHolderView : View
    {
        private readonly Func<int> _getBottomOffset;
        private readonly Func<object, UIView> _createContent;

        private UIView _current;

        private readonly Dictionary<object, UIView> _reuseCache = new Dictionary<object, UIView>();

        public NovelView[] OpenedNovels => _reuseCache.Values.Where(x => x is NovelView).Cast<NovelView>().ToArray();

        public ContentHolderView(Func<int> getBottomOffset, Func<object, UIView> createContent)
        {
            _getBottomOffset = getBottomOffset;
            _createContent = createContent;   

            Initialize();
        }

        public UIView Current => _current;

        private void Initialize()
        {
            Layout = () =>
            {
                if (_current != null)
                {
                    _current.ChangeSize(Frame.Width, Frame.Height);
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