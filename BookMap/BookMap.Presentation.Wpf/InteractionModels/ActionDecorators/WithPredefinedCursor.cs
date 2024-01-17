using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WithPredefinedCursor : IExecutableInteraction.Decorator
    {
        private readonly Canvas _container;
        private readonly Cursor _style;
        private readonly IExecutableInteraction _interaction;

        public WithPredefinedCursor(
            Canvas container,
            IEnumerable<ICursor> cursors,
            Cursor style,
            IExecutableInteraction interaction) : base(interaction)
        {
            _container = container;
            _style = style;
            _interaction = interaction;

            _interaction.IsActive.DistinctUntilChanged().Subscribe(
                _ =>
                {
                    if (_interaction.IsActive.Value)
                    {
                        foreach (var cursor in cursors)
                        {
                            cursor.Hide();
                        }
                        _container.Cursor = _style;
                    }
                }
            );
        }
    }
}