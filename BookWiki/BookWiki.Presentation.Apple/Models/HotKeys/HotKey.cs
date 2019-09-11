using System;
using BookWiki.Presentation.Apple.Controllers;
using UIKit;

namespace BookWiki.Presentation.Apple.Models.HotKeys
{
    public class HotKey
    {
        public KeyCombination Combination { get; }

        public Action Action { get; }

        public HotKey(KeyCombination keyCombination, Action action)
        {
            Combination = keyCombination;
            Action = action;
        }

        public HotKey(Key key, Action action)
        {
            Combination = new KeyCombination(key);
            Action = action;
        }

        public HotKey WithControl()
        {
            return new HotKey(new KeyCombination(UIKeyModifierFlags.Control, Combination.KeyCommand), Action);
        }

        public HotKey WithCommand()
        {
            return new HotKey(new KeyCombination(UIKeyModifierFlags.Command, Combination.KeyCommand), Action);
        }

        public HotKey WithShift()
        {
            return new HotKey(new KeyCombination(UIKeyModifierFlags.Shift, Combination.KeyCommand), Action);
        }

        public override string ToString()
        {
            return $"{Combination} -> Action()";
        }

        public HotKey Clone()
        {
            return new HotKey(Combination.Clone(), action: Action);
        }
    }
}