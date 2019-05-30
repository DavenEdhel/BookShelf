using System.Linq;
using System.Threading.Tasks;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Main;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class SearchResultsView : View, IContentView
    {
        private readonly IQuery _searchQuery;
        private readonly TabsCollectionView _tabsCollectionView;
        private CollectionView _searchResultsCollection;
        private QueryView _header;
        private UILabel _progress;

        private int _page;

        public SearchResultsView(IQuery searchQuery, TabsCollectionView tabsCollectionView)
        {
            _searchQuery = searchQuery;
            _tabsCollectionView = tabsCollectionView;
            Initialize();
        }

        private void Initialize()
        {
            _header = new QueryView(_searchQuery);
            _header.ShouldBecomeFirstResponderOnClick = false;
            Add(_header);

            _progress = new UILabel();
            _progress.Text = _searchQuery.Results.Completion.ProgressPercentage;
            _progress.TextColor = UIColor.DarkGray;
            _progress.TextAlignment = UITextAlignment.Right;
            _progress.Font = UIFont.BoldSystemFontOfSize(18);
            Add(_progress);

            _searchResultsCollection = new CollectionView();
            _searchResultsCollection.IsOrderingEnabled = false;
            _searchResultsCollection.ScrolledToBottom += SearchResultsCollectionOnScrolledToBottom;
            Add(_searchResultsCollection);

            LoadNextPage();

            Layout = () =>
            {
                _header.ChangeSize(Frame.Width - 100, 50);
                _header.ChangePosition(0, 0);

                _progress.ChangeSize(100, 50);
                _progress.PositionToRight(this);

                _searchResultsCollection.ChangeSize(Frame.Width, Frame.Height);
                _searchResultsCollection.ChangePosition(0, _header.Frame.Bottom);
            };

            Layout();
        }

        private CollectionItem GetCollectionItem(SearchResult searchResult)
        {
            var content = new SearchResultItemView(searchResult) { OnSelected = x => _tabsCollectionView.SelectTab(x) };

            return new CollectionItem(content, new SpaceSeparatorView(20));
        }

        private async void LoadNextPage()
        {
            await new AsyncForEach<SearchResult>(_searchQuery.Results.Skip(_page * 20).Take(20))
                .Execute(
                    body: async result =>
                    {
                         await Task.Run(() => result.Findings.Take(5).ToArray());

                        _searchResultsCollection.Add(GetCollectionItem(result));
                    },
                    onProgress: () => _progress.Text = _searchQuery.Results.Completion.ProgressPercentage);

            _page++;
        }

        private void SearchResultsCollectionOnScrolledToBottom()
        {
            LoadNextPage();
        }

        public void Hide()
        {
            
        }

        public void Show()
        {
            
        }
    }
}