using BookWiki.Presentation.Apple.Controllers;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Models.HotKeys
{
    public class KeyCombination
    {
        private readonly Key _key;

        public KeyCombination(UIKeyModifierFlags keyModifier, NSString key)
        {
            _key = new Key(key);
            ModifierFlags = keyModifier;
        }

        public KeyCombination(Key key)
        {
            _key = key;
            ModifierFlags = 0;
        }

        public KeyCombination(Key key, UIKeyModifierFlags keyModifier)
        {
            _key = key;
            ModifierFlags = keyModifier;
        }

        public KeyCombination(UIKeyCommand keyModifier)
        {
            _key = new Key(keyModifier.Input);
            ModifierFlags = keyModifier.ModifierFlags;
        }

        public NSString KeyCommand => _key.KeyCommand;

        public UIKeyModifierFlags ModifierFlags { get; }

        public bool IsEnglish => Key.EnglishRussian.ContainsKey(_key);

        public override string ToString()
        {
            if (ModifierFlags == 0)
            {
                return _key.ToString();
            }

            return $"{ModifierFlags} + {_key}";
        }

        protected bool Equals(KeyCombination other)
        {
            return Equals(_key, other._key) && ModifierFlags == other.ModifierFlags;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyCombination)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_key != null ? _key.GetHashCode() : 0) * 397) ^ ModifierFlags.GetHashCode();
            }
        }

        public KeyCombination ToRussian()
        {
            return new KeyCombination(ModifierFlags, Key.EnglishRussian[_key].KeyCommand);
        }

        public KeyCombination Clone()
        {
            return new KeyCombination(new Key(_key.KeyCommand), ModifierFlags);
        }
    }
}