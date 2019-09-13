using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class TextViewErrorCollection : IErrorsCollection
    {
        private readonly UITextView _textView;

        private readonly Logger _logger = new Logger("Errors");

        public TextViewErrorCollection(UITextView textView)
        {
            _textView = textView;
        }

        public void Add(IRange error)
        {
            _textView.Add(new ErrorLineView(error, _textView));

            _logger.Info(error.ToFormattedString());
        }

        public void RemoveAll()
        {
            foreach (var errorLineView in _textView.Subviews.Where(x => x is ErrorLineView))
            {
                errorLineView.RemoveFromSuperview();
            }

            _logger.Info("Cleaned");
        }
    }
}