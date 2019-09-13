using System;
using System.Collections.Generic;
using System.Linq;
using BookMap.Presentation.Apple;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Models.Utils;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Main;
using CoreGraphics;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;
using Application = BookWiki.Presentation.Apple.Models.Application;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class SuggestionsBoxView : View
    {
        private readonly SpellChecker _spellChecker;
        private readonly Action<NSRange, string> _onChosen;
        private readonly Action _onLearned;
        private string _result;
        private CollectionItem[] _items;

        private const string AddToDict = "*добавить в словарь*";

        public SuggestionsBoxView(SpellChecker spellChecker, Action<NSRange, string> onChosen, Action onLearned = null)
        {
            _spellChecker = spellChecker;
            _onChosen = onChosen;
            _onLearned = onLearned ?? (() => {});

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

            _schemeForEditMode = _scheme.Clone();

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

                _onLearned();

                return;
            }

            _onChosen(_spellChecker.MisspelledWord.ToNsRange(), _result);

            Hide();

            return;
        }

        private HotKeyScheme _scheme;
        private CollectionView _suggestionsView;
        private HotKeyScheme _schemeForEditMode;

        public void Show()
        {
            Application.Instance.PauseSchemes();

            Application.Instance.RegisterSchemeForEditMode(_schemeForEditMode);
            Application.Instance.RegisterSchemeForViewMode(_scheme);

            var rootView = AppDelegate.MainWindow.RootViewController.View;
            var suggestionsSize = _suggestionsView.ChangeWidthAndLayout((float)rootView.Frame.Width / 3f);

            this.ChangeSize(suggestionsSize.Width, suggestionsSize.Height);
            this.PositionToCenterInside(rootView);

            rootView.Add(this);
        }

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_scheme);
            Application.Instance.UnregisterScheme(_schemeForEditMode);

            Application.Instance.ResumeSchemes();

            RemoveFromSuperview();
        }
    }
}