using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf.Models
{
    public interface IMutableContent<T> where T : new()
    {
        T Content { get; set; }
    }

    public class EmptyContent<T> : IMutableContent<T> where T : new()
    {
        public T Content
        {
            get
            {
                return new T();
            }
            set
            {
            }
        }
    }

    public interface INamed
    {
        string Name { get; }
    }

    public class MutableNamedContent<T> : IMutableContent<T>, INamed
        where T : new()
    {
        private readonly IMutableContent<T> _content;

        public MutableNamedContent(IMutableContent<T> content, string name)
        {
            Name = name;
            _content = content;
        }

        public T Content
        {
            get => _content.Content;
            set => _content.Content = value;

        }
        public string Name { get; }
    }

    public class JsonFile<T> : IMutableContent<T> where T : new()
    {
        private readonly IAbsolutePath _path;
        private T _content;

        public JsonFile(IAbsolutePath path)
        {
            _path = path;
        }

        public T Content
        {
            get
            {
                if (File.Exists(_path.FullPath))
                {
                    if (_content == null)
                    {
                        _content = JsonConvert.DeserializeObject<T>(File.ReadAllText(_path.FullPath));
                    }

                    return _content;
                }

                return new T();
            }

            set
            {
                _content = value;

                File.WriteAllText(_path.FullPath, JsonConvert.SerializeObject(_content, Formatting.Indented));
            }
        }
    }

    public static class MutableContentExtension
    {
        public static MutableNamedContent<T> Named<T>(this IMutableContent<T> self, string name) where T : new() =>
            new MutableNamedContent<T>(self, name);
    }

    public class ScopeDto
    {
        public string Tags { get; set; } = string.Empty;

        public bool IsChapterEnabled { get; set; } = true;

        public bool IsBookEnabled { get; set; } = true;

        public bool IsGlobalEnabled { get; set; } = true;

        public ScopeDto Clone(Action<ScopeDto> change)
        {
            var n = (ScopeDto)MemberwiseClone();
            change(n);

            return n;
        }
    }

    public class ArticlesScopeOnFileSystem
    {
        public ArticlesScopeOnFileSystem(IRelativePath novel, IRootPath root)
        {
            if (novel == null)
            {
                Chapter = new EmptyContent<ScopeDto>().Named("нет");
                Book = new EmptyContent<ScopeDto>().Named("нет");
                Global = new EmptyContent<ScopeDto>().Named("нет");
                return;
            }

            Chapter = new MutableNamedContent<ScopeDto>(
                new JsonFile<ScopeDto>(novel.Down("scope.json").AbsolutePath(root)),
                novel.Name.PlainText
            );

            if (novel.CanGoUp())
            {
                Book = new MutableNamedContent<ScopeDto>(
                    new JsonFile<ScopeDto>(novel.Up().Down("scope.json").AbsolutePath(root)),
                    novel.Up().Name.PlainText
                );
            }
            else
            {
                Book = new EmptyContent<ScopeDto>().Named("нет");
                Global = new EmptyContent<ScopeDto>().Named("нет");
                return;
            }

            if (novel.Up().CanGoUp())
            {
                Global = new MutableNamedContent<ScopeDto>(
                    new JsonFile<ScopeDto>(novel.Up().Up().Down("scope.json").AbsolutePath(root)),
                    novel.Up().Up().Name.PlainText
                );
            }
            else
            {
                Global = new EmptyContent<ScopeDto>().Named("нет");
            }
        }

        public MutableNamedContent<ScopeDto> Global { get; }
        public MutableNamedContent<ScopeDto> Book { get; }
        public MutableNamedContent<ScopeDto> Chapter { get; }
    }

    public interface IArticleScope
    {
        string[] Scope { get; }

        Subject<Unit> ScopeChanged { get; }
    }

    public class ArticlesScope : IArticleScope
    {
        private readonly ArticlesScopeOnFileSystem _persistentScope;

        public ArticlesScope(ArticlesScopeOnFileSystem persistentScope)
        {
            _persistentScope = persistentScope;
        }

        public string[] Scope
        {
            get
            {
                var totalScope = Sources.JoinStringsWithoutSkipping(" ");

                return totalScope.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public Subject<Unit> ScopeChanged { get; } = new Subject<Unit>();

        private IEnumerable<string> Sources
        {
            get
            {
                if (_persistentScope.Chapter.Content.IsGlobalEnabled)
                {
                    yield return _persistentScope.Global.Content.Tags;
                }

                if (_persistentScope.Chapter.Content.IsBookEnabled)
                {
                    yield return _persistentScope.Book.Content.Tags;
                }

                if (_persistentScope.Chapter.Content.IsChapterEnabled)
                {
                    yield return _persistentScope.Chapter.Content.Tags;
                }
            }
        }
    }

    public class ArticlesScopeBehavior : IDisposable, IArticleScope
    {
        private readonly ScopeBox _global;
        private readonly ScopeBox _book;
        private readonly ScopeBox _chapter;
        private readonly ScopeBox[] _scopes;
        private readonly CompositeDisposable _scope = new CompositeDisposable();
        private readonly ArticlesScopeOnFileSystem _persistentScope;

        public ArticlesScopeBehavior(ScopeBox global, ScopeBox book, ScopeBox chapter, ArticlesScopeOnFileSystem persistentScope)
        {
            _global = global;
            _book = book;
            _chapter = chapter;

            _scopes = new ScopeBox[]
            {
                global,
                book,
                chapter
            };

            _persistentScope = persistentScope;

            global.QueryBox.Text = _persistentScope.Global.Content.Tags;
            global.Enabled.IsChecked = _persistentScope.Chapter.Content.IsGlobalEnabled;
            global.ScopeDefinition.Text = _persistentScope.Global.Name;
            book.QueryBox.Text = _persistentScope.Book.Content.Tags;
            book.Enabled.IsChecked = _persistentScope.Chapter.Content.IsBookEnabled;
            book.ScopeDefinition.Text = _persistentScope.Book.Name;
            chapter.QueryBox.Text = _persistentScope.Chapter.Content.Tags;
            chapter.Enabled.IsChecked = _persistentScope.Chapter.Content.IsChapterEnabled;
            chapter.ScopeDefinition.Text = _persistentScope.Chapter.Name;

            foreach (var scopeBox in _scopes)
            {
                scopeBox.Enabled.Checked += Changed;
                scopeBox.Enabled.Unchecked += Changed;
                scopeBox.QueryBox.TextChanged += Changed;
            }
        }

        private void Changed(object sender, RoutedEventArgs e)
        {
            ScopeChanged.OnNext(Unit.Default);
        }

        public IEnumerable<ScopeBox> Views => _scopes;

        public Subject<Unit> ScopeChanged { get; } = new Subject<Unit>();

        public string[] Scope
        {
            get
            {
                var totalScope = Views.Where(x => x.Enabled.IsChecked == true).Select(x => x.QueryBox.Text)
                    .JoinStringsWithoutSkipping(" ");

                return totalScope.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public void Dispose()
        {
            _scope.Dispose();

            foreach (var scopeBox in Views)
            {
                scopeBox.Enabled.Checked -= Changed;
                scopeBox.Enabled.Unchecked -= Changed;
                scopeBox.QueryBox.TextChanged -= Changed;
            }
        }

        public void Save()
        {
            _persistentScope.Chapter.Content = new ScopeDto()
            {
                Tags = _chapter.QueryBox.Text,
                IsChapterEnabled = _chapter.Enabled.IsChecked == true,
                IsBookEnabled = _book.Enabled.IsChecked == true,
                IsGlobalEnabled = _global.Enabled.IsChecked == true
            };

            _persistentScope.Book.Content = _persistentScope.Book.Content.Clone(
                x =>
                {
                    x.Tags = _book.QueryBox.Text;
                }
            );

            _persistentScope.Global.Content = _persistentScope.Book.Content.Clone(
                x =>
                {
                    x.Tags = _global.QueryBox.Text;
                }
            );
        }
    }
}