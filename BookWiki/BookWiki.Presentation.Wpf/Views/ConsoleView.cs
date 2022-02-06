using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ConsoleView : StackPanel
    {
        private readonly EnhancedRichTextBox _output;
        private readonly TextBox _input;

        public ConsoleView()
        {
            Height = 450;
            Orientation = Orientation.Vertical;
            Background = new SolidColorBrush(Colors.Azure);
            Margin = new Thickness(30, 0, 30, 0);

            var scrollView = new ScrollViewer();
            scrollView.Height = 420;
            _output = new EnhancedRichTextBox();
            
            scrollView.Content = _output;

            Children.Add(scrollView);
            Children.Add(_input = new TextBox()
            {
                Height = 30,
                FontSize = 15,
                FontFamily = new FontFamily("Times New Roman"),
                BorderBrush = new SolidColorBrush(Colors.AliceBlue)
            });

            _input.KeyDown += InputOnKeyDown;
        }

        private void InputOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                var q = new GenericQuery(_input.Text);

                var result = q.Execute();

                _output.Document.Blocks.Add(new Paragraph(new Run("*******")));

                foreach (var r in result)
                {
                    _output.Document.Blocks.Add(new Paragraph(new Run(r)));
                }

                _output.Document.Blocks.Add(new Paragraph(new Run("*******")));

                _input.Text = new Query(_input.Text).Command + " ";

                _output.ScrollToEnd();
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
                        "доступные комманды [имя], [место], [имена]"
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

    public class Query
    {
        public Query(string query)
        {
            var parts = query.Split(' ');

            Command = parts[0];

            if (parts.Length > 1)
            {
                Arguments = string.Join(" ", parts.Skip(1));
            }
        }

        public string Command { get; }

        public string Arguments { get; }
    }

    public class SearchByNamePartQuery
    {
        private readonly INovel _novel;
        private readonly string _query;

        public SearchByNamePartQuery(INovel novel, string query)
        {
            _novel = novel;
            _query = query;
        }

        public IEnumerable<string> Execute()
        {
            var allNames = new AllNames(_novel).ToArray();

            foreach (var nameWithContent in allNames.Where(x => x.IsValid))
            {
                if (nameWithContent.Names.Any(x => x.ToLower().Contains(_query.ToLower())))
                {
                    yield return nameWithContent.FullContent;
                }
            }
        }
    }

    public class SearchByNameGroupQuery
    {
        private readonly INovel _novel;
        private readonly string _query;

        public SearchByNameGroupQuery(INovel novel, string query)
        {
            _novel = novel;
            _query = query;
        }

        public IEnumerable<string> Execute()
        {
            var allNames = new NamesByGroups(_novel).ToArray();

            foreach (var nameWithContent in allNames.Where(x => x.IsValid))
            {
                if (nameWithContent.Group.ToLower().Contains(_query.ToLower()))
                {
                    foreach (var groupedName in nameWithContent)
                    {
                        yield return groupedName.FullContent;
                    }
                }
            }
        }
    }

    public class NamesWithFullContent
    {
        public NamesWithFullContent(string p)
        {
            try
            {
                var nameAndContent = p.Split('-', '–');

                if (nameAndContent.Length == 1)
                {
                    IsValid = false;

                    return;
                }

                var names = nameAndContent[0];

                if (string.IsNullOrWhiteSpace(names))
                {
                    names = nameAndContent[1];
                }

                var namesSplitted = names.Split(' ');

                Names = namesSplitted.Select(x => x.Trim()).ToArray();

                FullContent = p;

                IsValid = true;
            }
            catch (Exception e)
            {
                IsValid = false;
            }
        }

        public string[] Names { get; set; }

        public string FullContent { get; set; }

        public bool IsValid { get; set; }

        public string FullName => string.Join(" ", Names);
    }

    public class GroupedNames : IEnumerable<NamesWithFullContent>
    {
        private readonly IEnumerable<string> _ps;

        public GroupedNames(string group, IEnumerable<string> ps)
        {
            Group = @group;
            _ps = ps;
        }

        public string Group { get; }

        public bool IsValid { get; set; } = true;

        public IEnumerator<NamesWithFullContent> GetEnumerator()
        {
            foreach (var p in _ps)
            {
                yield return new NamesWithFullContent(p);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NamesByGroups : IEnumerable<GroupedNames>
    {
        private readonly INovel _novel;

        public NamesByGroups(INovel novel)
        {
            _novel = novel;
        }

        public IEnumerator<GroupedNames> GetEnumerator()
        {
            var document = new DocumentFlowContentFromTextAndFormat(_novel.Content, _novel.Format);

            var result = new List<List<IParagraph>>();

            var section = new List<IParagraph>();

            foreach (var p in document.Paragraphs)
            {
                if (p.FormattingStyle == TextStyle.Right)
                {
                    result.Add(section);
                    section = new List<IParagraph>();
                    section.Add(p);
                }
                else
                {
                    section.Add(p);
                }
            }

            result.Add(section);

            foreach (var p in result.Where(x => x.Count > 1 && x.Any(y => y.FormattingStyle == TextStyle.Right)))
            {
                yield return new GroupedNames(p.First(x => x.FormattingStyle == TextStyle.Right).GetAllText(), p.Skip(1).Select(x => x.GetAllText()).ToArray());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class AllNames : IEnumerable<NamesWithFullContent>
    {
        private readonly INovel _novel;

        public AllNames(INovel novel)
        {
            _novel = novel;
        }

        public IEnumerator<NamesWithFullContent> GetEnumerator()
        {
            var document = new DocumentFlowContentFromTextAndFormat(_novel.Content, _novel.Format);

            var content = document.Paragraphs.Where(x => x.FormattingStyle == TextStyle.None);

            foreach (var p in content)
            {
                yield return new NamesWithFullContent(p.GetAllText());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}