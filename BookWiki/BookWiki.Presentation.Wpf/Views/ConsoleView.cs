﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Search;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ConsoleView : StackPanel
    {
        private readonly EnhancedRichTextBox _output;
        private readonly TextBox _input;
        private readonly ScrollViewer _scrollView;
        private readonly LogSourceHolder _consoleLogSource = new LogSourceHolder();

        public ConsoleView()
        {
            Height = 450;
            Orientation = Orientation.Vertical;
            Background = new SolidColorBrush(Colors.Azure);

            _scrollView = new ScrollViewer();
            _scrollView.Height = 420;
            _output = new EnhancedRichTextBox();
            
            _scrollView.Content = _output;

            Children.Add(_scrollView);
            Children.Add(_input = new TextBox()
            {
                Height = 30,
                FontSize = 15,
                FontFamily = new FontFamily("Times New Roman"),
                BorderBrush = new SolidColorBrush(Colors.AliceBlue)
            });

            _input.KeyDown += InputOnKeyDown;

            _consoleLogSource.LogSource = BookShelf.Instance.PageConfig.Current.IsLoggingOn ? (ILogSource)new ConsoleLogSource(_output, Dispatcher.CurrentDispatcher) : new EmptyLogSource();
        }

        public void Start()
        {
            _consoleLogSource.Start();
        }

        public void Stop()
        {
            _consoleLogSource.Stop();
        }

        public void SetHeight(double value)
        {
            if (value == 0)
            {
                value = 900;
            }

            Height = value;
            _scrollView.Height = value - 30;
        }

        private void InputOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                var queryString = new Query(_input.Text);

                if (queryString.Command == "лог")
                {
                    if (queryString.Arguments == "показывать")
                    {
                        _consoleLogSource.LogSource = new ConsoleLogSource(_output, Dispatcher.CurrentDispatcher);
                        BookShelf.Instance.PageConfig.Current.IsLoggingOn = true;
                        _input.Text = string.Empty;
                        return;
                    }

                    if (queryString.Arguments == "скрыть")
                    {
                        _consoleLogSource.LogSource = new EmptyLogSource();
                        BookShelf.Instance.PageConfig.Current.IsLoggingOn = false;
                        _input.Text = string.Empty;
                        return;
                    }
                }

                var q = new GenericQuery(_input.Text);

                var result = q.Execute();

                _output.Document.Blocks.Add(new Paragraph(new Run("*******")));

                foreach (var r in result)
                {
                    _output.Document.Blocks.Add(new Paragraph(new Run(r)));
                }

                _output.Document.Blocks.Add(new Paragraph(new Run("*******")));

                _input.Text = string.Empty;
            }
        }
    }

    public class GenericQuery
    {
        private readonly string _query;

        public GenericQuery(string query)
        {
            _query = query;
        }

        public IEnumerable<string> Execute()
        {
            var q = new Query(_query);

            switch (q.Command)
            {
                case "имя":
                    return new SearchByNamePartQuery(Names, q.Arguments).Execute();
                case "место":
                    return new SearchByNameGroupQuery(Names, q.Arguments).Execute();
                case "имена":
                    return new AllNames(Names).Where(x => x.IsValid).Select(x => x.FullName);
                default:
                    return new List<string>()
                    {
                        "комманда не распознана",
                        "доступные комманды [имя], [место], [имена]",
                        "лог показывать",
                        "лог скрыть"
                    };
            }
        }

        private INovel Names
        {
            get
            {
                var namesPath = BookShelf.Instance.Root.GetAllLeafs()
                    .First(x => x.Path.FullPath.Contains(@"Материалы\Имена"));

                var novel = BookShelf.Instance.Read(namesPath.Path.RelativePath(BookShelf.Instance.RootPath));

                return novel;
            }
        }
    }
}