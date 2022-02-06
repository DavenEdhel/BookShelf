using System.Linq.Expressions;

namespace BookWiki.Presentation.Wpf.Models
{
    public class AppConfigDto
    {
        public string Root { get; set; }

        public string LibraryPath { get; set; }

        public TabDto[] Tabs { get; set; } = new TabDto[0];

        public string BackupPath { get; set; }
    }
}