using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Views.Common;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Apple.Views.Main
{
    public class TabsCollectionView : View
    {
        private readonly ILibrary _library;
        private CollectionView _collectionView;

        public event Action<object> OnTabSelected = delegate {  };

        public TabsCollectionView(ILibrary library)
        {
            _library = library;
            Initialize();
        }

        private void Initialize()
        {
            _collectionView = new CollectionView();
            Add(_collectionView);

            Layout = () =>
            {
                _collectionView.ChangeSize(Frame.Width, Frame.Height);
                _collectionView.ChangePosition(0, 0);
            };

            Layout();
        }

        public void Initialize(IContent[] articles)
        {
            var path = new UserFolderPath();

            var fileSystemNode = new FileSystemNode(path.FullPath);

            _collectionView.Add(new CollectionItem(new TabView(fileSystemNode) { IsDeletable = false, OnSelected = OnTabSelected }, new HorizontalSeparatorView()), animated: false);

            var collectionItems = new List<CollectionItem>();

            for (int i = 0; i < articles.Length; i++)
            {
                var article = articles[i];

                collectionItems.Add(new CollectionItem(new TabView(article, i) { OnSelected = OnTabSelected, IsDefaultTab = false }, new HorizontalSeparatorView()));
            }

            _collectionView.AddRangeWithoutAnimation(collectionItems.ToArray());
        }

        public void SelectFiles()
        {
            _collectionView.Select(_collectionView[0]);
        }

        public void SelectTab(int number)
        {
            _collectionView.Select(_collectionView[number]);
        }

        public void SelectTab(string title)
        {
            var item = _collectionView.Items.FirstOrDefault(x =>
                x.Content is IPageData && x.Content.CastTo<IPageData>().EqualsTo(title));

            if (item != null)
            {
                _collectionView.Select(item);
            }
        }

        public void SelectTab(IContent content)
        {
            var item = _collectionView.Items.FirstOrDefault(x => x.Content is IPageData && x.Content.CastTo<IPageData>().EqualsTo(content));

            if (item != null)
            {
                _collectionView.Select(item);
            }
            else
            {
                var index = _collectionView.Items.Where(x => x.Content.CastTo<TabView>().Data is IContent).MaxOrDefault(x => x.Content.CastTo<TabView>().Index) + 1;

                item = new CollectionItem(new TabView(content, index) {OnSelected = OnTabSelected, IsDefaultTab = false }, new HorizontalSeparatorView());

                _collectionView.Add(item);

                _collectionView.Select(item);
            }
        }

        public void ShowSearchResult(IQuery searchQuery)
        {
            var index = _collectionView.Items.Where(x => x.Content.CastTo<TabView>().Data is IQuery).MaxOrDefault(x => x.Content.CastTo<TabView>().Index) + 1;

            var item = new CollectionItem(new TabView(searchQuery, index) {OnSelected = OnTabSelected, IsDefaultTab = false }, new HorizontalSeparatorView());

            _collectionView.Add(item);

            _collectionView.Select(item);
        }

        public IEnumerable<TabView> OpenedCustomTabs
        {
            get
            {
                foreach (var content in _collectionView.Items)
                {
                    if (content.Content is TabView pageDataContainer)
                    {
                        if (pageDataContainer.IsDefaultTab == false)
                        {
                            yield return pageDataContainer;
                        }
                    }
                }
            }
        }
    }
}