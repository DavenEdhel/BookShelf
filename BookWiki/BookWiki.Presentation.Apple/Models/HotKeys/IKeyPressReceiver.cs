using UIKit;

namespace BookWiki.Presentation.Apple.Controllers
{
    public interface IKeyPressReceiver
    {
        void ProcessKey(UIKeyCommand cmd);
    }
}