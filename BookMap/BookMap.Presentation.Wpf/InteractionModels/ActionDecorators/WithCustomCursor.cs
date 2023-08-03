using System;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WithCustomCursor : IExecutableInteraction.Decorator
    {
        private readonly Canvas _container;
        private readonly string _cursorLabel;
        private readonly LabeledCursor _cursor;
        private readonly IExecutableInteraction _interaction;

        public WithCustomCursor(
            Canvas container,
            string cursorLabel,
            LabeledCursor cursor,
            IExecutableInteraction interaction) : base(interaction)
        {
            _container = container;
            _cursorLabel = cursorLabel;
            _cursor = cursor;
            _interaction = interaction;

            _interaction.IsActive.DistinctUntilChanged().Subscribe(
                _ =>
                {
                    if (_interaction.IsActive.Value)
                    {
                        _cursor.Show();
                        _cursor.Title = _cursorLabel;
                        _container.Cursor = Cursors.None;
                    }
                }
            );
        }
    }
}