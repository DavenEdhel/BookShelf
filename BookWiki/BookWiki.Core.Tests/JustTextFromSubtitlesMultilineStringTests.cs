using BookWiki.Core.Utils.TextModels;
using Keurig.Tests.Common.Utils;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace BookWiki.Core.Tests
{
    public class JustTextFromSubtitlesMultilineStringTests
    {
        [Test]
        public void Smoke()
        {
            var cleaned = new JustTextFromSubtitlesMultilineString(
                [
"4",
"00:00:39,790 --> 00:00:41,292",
"[coughing]",
"",
"5",
"00:00:53,721 --> 00:00:56,265",
"Do not be afraid, my love.",
"",
"6",
"00:00:56,974 --> 00:00:58,851",
"I will see you [crying] again.",
"",
"8",
"00:01:07,401 --> 00:01:08,611",
"- [shouts]",

                ]
                ).ToArray();

            cleaned.CollectionIs([
                "Do not be afraid, my love.",
                "I will see you again."
                ]);
        }
    }
}