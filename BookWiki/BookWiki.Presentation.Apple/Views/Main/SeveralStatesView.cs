using System;
using System.Linq;
using BookWiki.Core.Utils;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class SeveralStatesView : View
    {
        private readonly string[] _states;
        private UIButton _button;
        private int _currentIndex = 0;

        public string Current
        {
            get => _states[CurrentIndex];
            set => CurrentIndex = _states.IndexOf(x => x == value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                var oldIndex = _currentIndex;

                _currentIndex = value;

                _button.SetTitle(Current, UIControlState.Normal);

                if (oldIndex != _currentIndex)
                {
                    Changed();
                }
            }
        }

        public event Action Changed = delegate { };

        public SeveralStatesView(params string[] states)
        {
            _states = states;

            InitializeView();
        }

        private void InitializeView()
        {
            UserInteractionEnabled = true;

            _button = new UIButton(UIButtonType.RoundedRect);
            _button.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _button.SetTitle(_states.First(), UIControlState.Normal);
            _button.TouchUpInside += OnTouchUpInside;
            Add(_button);

            Layout = () => { _button.Frame = new CGRect(0, 0, Frame.Width, Frame.Height); };

            Layout();
        }

        private void OnTouchUpInside(object sender, EventArgs e)
        {
            CurrentIndex = (CurrentIndex + 1) % _states.Length;
        }
    }
}