using System;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class NamesWithFullContent
    {
        public NamesWithFullContent(string p)
        {
            try
            {
                var nameAndContent = p.Split('-', '–');

                if (nameAndContent.Length == 1)
                {
                    IsValid = false;

                    return;
                }

                var names = nameAndContent[0];

                if (string.IsNullOrWhiteSpace(names))
                {
                    names = nameAndContent[1];
                }

                var namesSplitted = names.Split(' ');

                Names = namesSplitted.Select(x => x.Trim()).ToArray();

                FullContent = p;

                IsValid = true;
            }
            catch (Exception e)
            {
                IsValid = false;
            }
        }

        public string[] Names { get; set; }

        public string FullContent { get; set; }

        public bool IsValid { get; set; }

        public string FullName => string.Join(" ", Names);
    }
}