using BookWiki.Core.Files.PathModels;
using Newtonsoft.Json;

namespace BookWiki.Core.ViewModels
{
    public class JsonEditorState : IEditorState
    {
        private readonly Scheme _json;

        public JsonEditorState(IEditorState state)
        {
            _json = new Scheme()
            {
                IsEditing = state.IsEditing,
                LastCaretPosition = state.LastCaretPosition,
                ScrollPosition = state.ScrollPosition,
                NovelPathToLoad = state.NovelPathToLoad.FullPath
            };
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(_json);
        }

        public JsonEditorState(string json)
        {
            _json = JsonConvert.DeserializeObject<Scheme>(json);
        }

        public JsonEditorState(Scheme scheme)
        {
            _json = scheme;
        }

        public class Scheme
        {
            public string NovelPathToLoad { get; set; }

            public int ScrollPosition { get; set; }

            public int LastCaretPosition { get; set; }

            public bool IsEditing { get; set; }
        }

        public IRelativePath NovelPathToLoad => new FolderPath(_json.NovelPathToLoad);
        public int ScrollPosition => _json.ScrollPosition;
        public int LastCaretPosition => _json.LastCaretPosition;
        public bool IsEditing => _json.IsEditing;
    }
}