using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using Keurig.Tests.Common.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class IndexSequenceV2Tests
    {
        [Test]
        public void ShouldReturnIndexesOfAllX()
        {
            var text = "012XX5X78X";

            var s = new IndexesOfCharacters(text, 'X').ToArray();

            Claim.Equal(s.Length, 4);
            Claim.Equal(text[s[0]], 'X');
            Claim.Equal(text[s[1]], 'X');
            Claim.Equal(text[s[2]], 'X');
            Claim.Equal(text[s[3]], 'X');
        }

        [Test]
        public void ShouldSplitToIntervalsCorrectly()
        {
            var text = "Ðîãà, êîïûòà äà õâîñòû";

            var s = new IntervalsBetweenCharacters(text, TextParts.NotALetterOrNumber).ToArray();

            Claim.Equal(text.Substring(s[0]), "Ðîãà");
            Claim.Equal(text.Substring(s[1]), "êîïûòà");
            Claim.Equal(text.Substring(s[2]), "äà");
            Claim.Equal(text.Substring(s[3]), "õâîñòû");
        }

        [Test]
        public void ShouldSplitToSubstringsCorrectly()
        {
            var items = new List<OffsetText>()
            {
                new OffsetText()
                {
                    Offset = 100,
                    Text = "123 456 "
                },
                new OffsetText()
                {
                    Offset = 110,
                    Text = "789"
                },
                new OffsetText()
                {
                    Offset = 120,
                    Text = " 123"
                },
                new OffsetText()
                {
                    Offset = 130,
                    Text = " "
                },
                new OffsetText()
                {
                    Offset = 140,
                    Text = "456 "
                },
                new OffsetText()
                {
                    Offset = 150,
                    Text = "123"
                },
                new OffsetText()
                {
                    Offset = 160,
                    Text = "4"
                },
                new OffsetText()
                {
                    Offset = 170,
                    Text = "56"
                },
            };

            var substrings = new PunctuationSeparatedEnumerationV2(items).ToArray();

            Claim.Equal(substrings[0].Text, "123");
            Claim.Equal(substrings[1].Text, "456");
            Claim.Equal(substrings[2].Text, "789");
            Claim.Equal(substrings[3].Text, "123");
            Claim.Equal(substrings[4].Text, "456");
            Claim.Equal(substrings[5].Text, "123456");
        }

        [Test]
        public void Chapter()
        {
            var c = "Ник вылез, снова хлопнул дверью аааа.";

            var items = new PunctuationSeparatedEnumerationV2(new []
            {
                new OffsetText()
                {
                    Offset = 2,
                    Text = c
                }
            }).ToArray();

            (items.Last().EndIndex - items.Last().StartIndex).ClaimEqual(4);
        }

        [Test]
        public void SpecificCase()
        {
            var json =
                "[{\"Text\":\"- Проверил я, проверил, - пробурчал Ник в ответ, а потом добавил раздражённое: - сэр. Было пол\",\"Offset\":17598},{\"Text\":\"а\",\"Offset\":17694},{\"Text\":\"бака. Только вот, сэр, знаете в чём проблема? - Ник на\",\"Offset\":17697},{\"Text\":\"та\",\"Offset\":17753},{\"Text\":\"чинал медленно закипать. - Хуй знает, какой здесь расход! Может как у танка, сто литров на сто километров. И объём бака сложно определить без эстакады и разбора, блять!\",\"Offset\":17757}]";

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OffsetText>>(json);

            var items = new PunctuationSeparatedEnumerationV2(obj).ToArray();

            var e = items[13];

            var i = 32;


        }

        [Test]
        public void ShouldSplitToSubstringsCorrectly2()
        {
            var items = new List<OffsetText>()
            {
                new OffsetText()
                {
                    Offset = 150,
                    Text = "123"
                },
                new OffsetText()
                {
                    Offset = 160,
                    Text = "4"
                },
                new OffsetText()
                {
                    Offset = 170,
                    Text = "56"
                },
            };

            var substrings = new PunctuationSeparatedEnumerationV2(items).ToArray();

            Claim.Equal(substrings.Length, 1);
            Claim.Equal(substrings[0].Text, "123456");
        }
    }
}