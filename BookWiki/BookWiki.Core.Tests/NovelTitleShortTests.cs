using System.Collections.Generic;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.Tests.Common.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class NovelTitleShortTests
    {
        [Test]
        public void Smoke()
        {
            var a1 = new NovelTitleShort(new PathFake("Глава 1. Ааа"));
            var a2 = new NovelTitleShort(new PathFake("Заметки"));
            var a3 = new NovelTitleShort(new PathFake("Глава 1. Ааа 02"));

            var list = new List<NovelTitleShort>()
            {
                a2, a3, a1
            };

            list.Sort();

            Claim.True(list[0].Value == "Глава 1. Ааа");
            Claim.True(list[1].Value == "Глава 1. Ааа 02");
            Claim.True(list[2].Value == "Заметки");
        }
    }
}