using System.Linq;
using BookWiki.Presentation.Apple.Models.HotKeys;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class HotKeyScheme
    {
        private Keyboard _keyboard;
        private readonly HotKey[] _hotKeys;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;

                    if (value)
                    {
                        Enable();
                    }
                    else
                    {
                        Disable();
                    }
                }
            }
        }

        public HotKeyScheme SameWithControl => new HotKeyScheme(_hotKeys.Select(x => x.WithControl()).ToArray());
        public HotKeyScheme SameWithCommand => new HotKeyScheme(_hotKeys.Select(x => x.WithCommand()).ToArray());

        public HotKeyScheme(params HotKey[] hotKeys)
        {
            _hotKeys = hotKeys;
        }

        private void Enable()
        {
            foreach (var hotKey in _hotKeys)
            {
                _keyboard.RegisterHandler(hotKey.Combination, hotKey.Action);

                if (hotKey.Combination.IsEnglish)
                {
                    _keyboard.RegisterHandler(hotKey.Combination.ToRussian(), hotKey.Action);
                }
                
            }
        }

        private void Disable()
        {
            foreach (var hotKey in _hotKeys)
            {
                _keyboard.Unregister(hotKey.Combination, hotKey.Action);

                if (hotKey.Combination.IsEnglish)
                {
                    _keyboard.Unregister(hotKey.Combination.ToRussian(), hotKey.Action);
                }
            }
        }

        public void AssignToKeyboard(Keyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public override string ToString()
        {
            return string.Join('\n', _hotKeys.Select(x => x.ToString()));
        }

        public HotKeyScheme Clone()
        {
            return new HotKeyScheme(_hotKeys.Select(x => x.Clone()).ToArray());
        }
    }
}