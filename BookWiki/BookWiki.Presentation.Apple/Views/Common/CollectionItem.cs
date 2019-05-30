using System;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class CollectionItem : IComparable<CollectionItem>
    {
        public UIView Content { get; }

        public UIView Separator { get; }

        public CollectionItem(UIView content, UIView separator)
        {
            Content = content;
            Separator = separator;
        }

        public int CompareTo(CollectionItem other)
        {
            if (Content is IComparable comparableContent)
            {
                return comparableContent.CompareTo(other.Content);
            }

            return 0;
        }
    }
}