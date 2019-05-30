using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Main;
using CoreGraphics;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class AutocorectionBoxView : View
    {
        private readonly SpellChecker _spellChecker;
        private readonly Action<NSRange, string> _onChosen;
        private string _result;
        private CollectionItem[] _items;

        private const string AddToDict = "*добавить в словарь*";

        public AutocorectionBoxView(SpellChecker spellChecker, Action<NSRange, string> onChosen)
        {
            _spellChecker = spellChecker;
            _onChosen = onChosen;

            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;
            Layer.BorderWidth = 1;
            Layer.BorderColor = UIColor.LightGray.CGColor;

            _scheme = new HotKeyScheme(
                new HotKey(Key.Escape, Hide),
                new HotKey(Key.Enter, ApplySelected),
                new HotKey(Key.ArrowDown, SelectNext),
                new HotKey(Key.ArrowUp, SelectPrev),
                
                new HotKey(new Key("h"), () => TapOn(0)),
                new HotKey(new Key("j"), () => TapOn(1)),
                new HotKey(new Key("k"), () => TapOn(2)),
                new HotKey(new Key("l"), () => TapOn(3)),
                new HotKey(new Key(";"), () => TapOn(4)));

            var items = _spellChecker.Guesses.Select(suggestion =>
            {
                var collectionItem = new CollectionItem(
                    new SuggestionItemView(suggestion) { OnSelected = ItemSelected },
                    new HorizontalSeparatorView()
                );

                return collectionItem;
            }).ToArray();

            _items = new CollectionItem[]
                {
                    new CollectionItem(
                        new SuggestionItemView(AddToDict) {OnSelected = ItemSelected},
                        new HorizontalSeparatorView()
                    ),
                }
                .And(items)
                .ToArray();

            _suggestionsView = new CollectionView();
            _suggestionsView.IsOrderingEnabled = false;
            _suggestionsView.AddRangeWithoutAnimation(_items);
            
            Add(_suggestionsView);

            _suggestionsView.Select(_items.SecondOrFirst());
        }

        private void SelectPrev()
        {
            var selectedIndex = _items.IndexOf(x => x.Content.CastTo<ISelectable>().IsSelected);

            var prev = _items.ElementAtOrDefault(selectedIndex - 1);

            if (prev != null)
            {
                _suggestionsView.Select(prev);
            }
        }

        private void SelectNext()
        {
            var selectedIndex = _items.IndexOf(x => x.Content.CastTo<ISelectable>().IsSelected);

            var next = _items.ElementAtOrDefault(selectedIndex + 1);

            if (next != null)
            {
                _suggestionsView.Select(next);
            }
        }

        private void TapOn(int index)
        {
            var item = _items.ElementAtOrDefault(index);

            if (item != null)
            {
                if (item.Content is ISelectable selectableItem)
                {
                    if (selectableItem.IsSelected)
                    {
                        ApplySelected();
                    }
                    else
                    {
                        _suggestionsView.Select(item);
                    }
                }
            }
        }

        private void ItemSelected(string content)
        {
            _result = content;
        }

        private void ApplySelected()
        {
            if (_result == AddToDict)
            {
                _spellChecker.Learn();

                Hide();

                return;
            }

            _onChosen(_spellChecker.MisspelledWord, _result);

            Hide();

            return;
        }

        private IKeyboardListener _keyboardListener;
        private HotKeyScheme _scheme;
        private CollectionView _suggestionsView;

        public void Show(UIView parent, CGPoint position)
        {
            _keyboardListener = parent as IKeyboardListener;

            _keyboardListener?.Pause();

            Application.Instance.RegisterSchemeForEditMode(_scheme);
            Application.Instance.RegisterSchemeForViewMode(_scheme);

            var suggestionsSize = _suggestionsView.ChangeWidthAndLayout((float)parent.Frame.Width / 3f);

            Frame = new CGRect(position, suggestionsSize);

            parent.Add(this);
        }

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_scheme);

            _keyboardListener?.Resume();

            RemoveFromSuperview();
        }
    }
}