using System;
using BookWiki.Core;
using BookWiki.Core.Search;
using BookWiki.Presentation.Apple.Extentions;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class QueryView : View
    {
        private readonly IQuery _query;
        private UITextView _input;

        public event Action Clicked = delegate { };
        public event Action Changed = delegate { };
        public event Action OnFocused = delegate { };
        public event Action OnFocusLeaved = delegate { };

        public bool ShouldBecomeFirstResponderOnClick { get; set; } = true;

        public string QueryAsText => _input.Text;

        public override bool IsFirstResponder => _input.IsFirstResponder;

        public QueryView(ILibrary library) : this(new EqualityQuery(new SearchQuery(library, "")))
        {
        }

        public QueryView(IQuery query)
        {
            _query = query;

            Initialize();
        }

        private void Initialize()
        {
            UserInteractionEnabled = true;

            _input = new UITextView();
            _input.Font = UIFont.BoldSystemFontOfSize(20);
            _input.AutocapitalizationType = UITextAutocapitalizationType.None;
            _input.Text = _query.Title;
            _input.Changed += InputOnChanged;
            _input.Started += InputOnStarted;
            _input.Ended += InputOnEnded;
            Add(_input);

            Layout = () =>
            {
                _input.ChangeSize(Frame.Width, Frame.Height);
                _input.ChangePosition(0, 0);
            };

            Layout();

            AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                if (ShouldBecomeFirstResponderOnClick)
                {
                    _input.BecomeFirstResponder();
                }

                Clicked();
            }));
        }

        private void InputOnEnded(object sender, EventArgs e)
        {
            OnFocusLeaved();
        }

        private void InputOnStarted(object sender, EventArgs e)
        {
            OnFocused();
        }

        private void InputOnChanged(object sender, EventArgs e)
        {
            Changed();
        }

        public override bool ResignFirstResponder()
        {
            return _input.ResignFirstResponder();
        }

        public override bool BecomeFirstResponder()
        {
            return _input.BecomeFirstResponder();
        }
    }
}