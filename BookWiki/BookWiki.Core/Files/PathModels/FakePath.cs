namespace BookWiki.Core.Files.PathModels
{
    public class FakePath : IPath
    {
        private readonly string _id;

        public FakePath(string id)
        {
            _id = id;
        }

        public IFileName Name => new FakeFileName() {PlainText = _id};

        public IExtension Extension => new Extension(new EmptySubstring());

        public string FullPath => Name.PlainText;

        public bool EqualsTo(IPath path)
        {
            if (path is FakePath mockPath)
            {
                return mockPath._id == _id;
            }

            return false;
        }

        public IPartsSequence Parts { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return EqualsTo(obj as IPath);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}