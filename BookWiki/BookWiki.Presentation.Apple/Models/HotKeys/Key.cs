using System;
using System.Collections.Generic;
using BookWiki.Presentation.Apple.Models.HotKeys;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class Key
    {
        public static readonly Key Space = new Key(new NSString(" "));
        public static readonly Key Enter = new Key(new NSString("\r"));
        public static readonly Key Escape = new Key(UIKeyCommand.Escape);
        public static readonly Key ArrowDown = new Key(UIKeyCommand.DownArrow);
        public static readonly Key ArrowUp = new Key(UIKeyCommand.UpArrow);

        public static readonly Dictionary<Key, Key> EnglishRussian = new Dictionary<Key, Key>()
        {
            [new Key("y")] = new Key("н"),
            [new Key("u")] = new Key("г"),
            [new Key("i")] = new Key("ш"),
            [new Key("o")] = new Key("щ"),
            [new Key("p")] = new Key("з"),
            [new Key("f")] = new Key("а"),
            [new Key("h")] = new Key("р"),
            [new Key("j")] = new Key("о"),
            [new Key("k")] = new Key("л"),
            [new Key("l")] = new Key("д"),
            [new Key(";")] = new Key("ж"),
            [new Key("n")] = new Key("т")
        };

        public Key(string key)
        {
            KeyCommand = (NSString) key;
        }

        public Key(NSString key)
        {
            KeyCommand = key;
        }

        public NSString KeyCommand { get; }

        public override string ToString()
        {
            if (ReferenceEquals(Space, this))
            {
                return "Space";
            }

            if (ReferenceEquals(Enter, this))
            {
                return "Enter";
            }

            if (ReferenceEquals(Escape, this))
            {
                return "Escape";
            }

            return KeyCommand.ToString();
        }

        protected bool Equals(Key other)
        {
            return Equals(KeyCommand, other.KeyCommand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Key) obj);
        }

        public override int GetHashCode()
        {
            return (KeyCommand != null ? KeyCommand.GetHashCode() : 0);
        }
    }
}