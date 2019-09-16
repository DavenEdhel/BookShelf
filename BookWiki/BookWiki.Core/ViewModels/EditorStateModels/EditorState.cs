namespace BookWiki.Core.ViewModels
{
    public static class EditorState
    {
        public static IEditorState Create(JsonEditorState.Scheme scheme)
        {
            return new JsonEditorState(scheme);
        }

        public static IEditorState Create(string json)
        {
            return new JsonEditorState(json);
        }

        public static string ToJson(this IEditorState state)
        {
            return new JsonEditorState(state).ToJson();
        }
    }
}