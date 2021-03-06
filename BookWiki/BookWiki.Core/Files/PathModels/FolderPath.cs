﻿using System.IO;
using System.Linq;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.PathModels
{
    public class FolderPath : IPath
    {
        private readonly string _path;

        public FolderPath(string path)
        {
            _path = path;
            Parts = new PartsSequence(new RunOnceSequence<ITextRange>(new StringPathPartsSequence(path)));
            Name = new FileName(Parts);
            Extension = new Extension(Parts);
        }

        public FolderPath(IPath root, IFileName folderName, IExtension extension) : this(Path.Combine(root.FullPath, $"{folderName.PlainText}{(extension.Type != NodeType.Directory ? "." : string.Empty)}{extension.PlainText}"))
        {
        }

        public IFileName Name { get; }

        public IExtension Extension { get; }

        public string FullPath => _path;

        public bool EqualsTo(IPath path)
        {
            return _path.Equals(path.FullPath);
        }

        public IPartsSequence Parts { get; }
    }
}