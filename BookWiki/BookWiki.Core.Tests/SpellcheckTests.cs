using System.Threading.Tasks;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.Tests.Common.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class SpellcheckTests
    {
        [Test]
        public async Task SpellcheckShouldWork()
        {
            var lex = new WordCollectionFromLex(string.Empty, new FakeFileProvider(new[]
            {
                "ааб",
                "аб",
                "абв"
            }));

            await lex.Load();

            var spellCheck = new SpellCheckV2(lex);

            Claim.False(spellCheck.IsCorrect("аа"));
            Claim.True(spellCheck.IsCorrect("аб"));
            Claim.True(spellCheck.IsCorrect("абв"));
            Claim.False(spellCheck.IsCorrect("абвг"));
        }

        [Test]
        public async Task LearnShouldWork()
        {
            var lex = new WordCollectionFromLex(string.Empty, new FakeFileProvider(new[]
            {
                "ааб",
                "аб",
                "абв"
            }));

            await lex.Load();

            var spellCheck = new SpellCheckV2(lex);

            Claim.False(spellCheck.IsCorrect("аа"));
            Claim.True(spellCheck.IsCorrect("аб"));
            Claim.True(spellCheck.IsCorrect("абв"));
            Claim.False(spellCheck.IsCorrect("абвг"));

            await lex.Learn("абвг");

            var spellCheck2 = new SpellCheckV2(lex);

            Claim.True(spellCheck2.IsCorrect("абвг"));
        }
    }
}