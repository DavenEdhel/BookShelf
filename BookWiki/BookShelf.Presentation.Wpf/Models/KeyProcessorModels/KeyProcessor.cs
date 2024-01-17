using System.Windows.Input;
using BookWiki.Presentation.Wpf.Models.QuickNavigationModels;

namespace BookWiki.Presentation.Wpf.Models.KeyProcessorModels
{
    public class KeyProcessor
    {
        private readonly QuickNavigationProcessor _quickNavigation = new QuickNavigationProcessor();
        private readonly QuickArticleProcessor _quickArticle = new QuickArticleProcessor();

        public bool Handle(KeyboardDevice keyboard)
        {
            if (keyboard.IsKeyDown(Key.LeftCtrl) && keyboard.IsKeyDown(Key.LeftShift) && keyboard.IsKeyDown(Key.N))
            {
                _quickNavigation.Process();
                return true;
            }

            if (keyboard.IsKeyDown(Key.LeftCtrl)  && keyboard.IsKeyDown(Key.N))
            {
                _quickArticle.Process();
                return true;
            }

            return false;
        }
    }
}