using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Presentation.Apple.Models.HotKeys;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class Keyboard : IKeyPressReceiver
    {
        public const string KeyCommandMethodName = "KeyCommand:";

        private readonly UIViewController _controller;

        private readonly List<UIKeyCommand> _activeCommands = new List<UIKeyCommand>();

        private readonly Dictionary<KeyCombination, List<Action>> _actions = new Dictionary<KeyCombination, List<Action>>();

        public Keyboard(UIViewController controller)
        {
            _controller = controller;
        }

        public void TurnOnCustomHandling()
        {
            foreach (var command in _activeCommands)
            {
                _controller.AddKeyCommand(command);
            }
        }

        public void TurnOffCustomHandling()
        {
            foreach (var command in _activeCommands)
            {
                _controller.RemoveKeyCommand(command);
            }
        }

        public void RegisterHandler(KeyCombination combination, Action action)
        {
            if (_actions.ContainsKey(combination))
            {
                _actions[combination].Add(action);
            }
            else
            {
                var command = UIKeyCommand.Create(combination.KeyCommand, combination.ModifierFlags, new Selector(KeyCommandMethodName));

                _activeCommands.Add(command);
                _controller.AddKeyCommand(command);

                _actions.Add(combination, new List<Action>()
                {
                    action
                });
            }
        }

        public void Unregister(KeyCombination combination, Action action)
        {
            foreach (var combinationAndListOfActions in _actions.Where(x => x.Key.Equals(combination)).ToArray())
            {
                var actionList = combinationAndListOfActions.Value;

                actionList.Remove(action);

                TryToCleanUpCommand(combinationAndListOfActions);
            }
        }

        private void TryToCleanUpCommand(KeyValuePair<KeyCombination, List<Action>> combinationAndListOfActions)
        {
            var actionList = combinationAndListOfActions.Value;

            if (actionList.Any() == false)
            {
                _actions.Remove(combinationAndListOfActions.Key);

                var command = _activeCommands.FirstOrDefault(x =>
                    x.Input == combinationAndListOfActions.Key.KeyCommand &&
                    x.ModifierFlags == combinationAndListOfActions.Key.ModifierFlags);

                _activeCommands.Remove(command);

                _controller.RemoveKeyCommand(command);
            }
        }

        void IKeyPressReceiver.ProcessKey(UIKeyCommand cmd)
        {
            var keyCombination = new KeyCombination(cmd);

            if (_actions.ContainsKey(keyCombination))
            {
                foreach (var action in _actions[keyCombination].ToArray())
                {
                    action();
                }
            }
        }
    }
}