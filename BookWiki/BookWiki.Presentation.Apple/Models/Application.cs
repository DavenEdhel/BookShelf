using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using BookWiki.Core.Logging;
using BookWiki.Presentation.Apple.Controllers;

namespace BookWiki.Presentation.Apple.Models
{
    public class Application
    {
        private readonly Logger _logger = new Logger("App");

        public static Application Instance { get; private set; }

        public bool IsInEditMode
        {
            get => _isInEditMode;
            private set
            {
                _isInEditMode = value;

                ModeChanged(_isInEditMode);
            }
        }

        public event Action<bool> ModeChanged = delegate { };

        private readonly List<HotKeyScheme> _schemesForEditMode = new List<HotKeyScheme>();
        private readonly List<HotKeyScheme> _schemesForViewMode = new List<HotKeyScheme>();

        public static void Run(Keyboard keyboard)
        {
            Instance = new Application(keyboard);
        }

        private readonly Keyboard _keyboard;
        private bool _isInEditMode = false;

        private Application(Keyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public void RegisterScheme(HotKeyScheme scheme)
        {
            RegisterSchemeForViewMode(scheme);
            RegisterSchemeForViewMode(scheme.SameWithCommand);
            RegisterSchemeForEditMode(scheme.SameWithCommand);
        }

        public void RegisterSchemeForEditMode(HotKeyScheme scheme)
        {
            _schemesForEditMode.Add(scheme);

            scheme.AssignToKeyboard(_keyboard);
            scheme.IsEnabled = IsInEditMode;
        }

        public void RegisterSchemeForViewMode(HotKeyScheme scheme)
        {
            _schemesForViewMode.Add(scheme);

            scheme.AssignToKeyboard(_keyboard);
            scheme.IsEnabled = !IsInEditMode;
        }

        public void UnregisterScheme(HotKeyScheme scheme)
        {
            scheme.IsEnabled = false;

            _schemesForEditMode.Remove(scheme);
            _schemesForViewMode.Remove(scheme);
        }

        public void ToEditMode()
        {
            _schemesForEditMode.ForEach(x => x.IsEnabled = true);
            _schemesForViewMode.ForEach(x => x.IsEnabled = false);

            IsInEditMode = true;

            _logger.Info($"Moved to edit mode. {string.Join('\n', _schemesForEditMode.Select(x => x.ToString()))}");
        }

        public void ToViewMode()
        {
            _schemesForEditMode.ForEach(x => x.IsEnabled = false);
            _schemesForViewMode.ForEach(x => x.IsEnabled = true);

            IsInEditMode = false;

            _logger.Info($"Moved to view mode. {string.Join('\n', _schemesForViewMode.Select(x => x.ToString()))}");
        }
    }
}