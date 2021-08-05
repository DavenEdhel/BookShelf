using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public abstract class BackgroundStrategy
    {
        protected UIImageView Item { get; set; }

        public virtual void Initialize(UIImageView item)
        {
            
        }

        public virtual void Render()
        {
            
        }

        public virtual void ApplyActiveColor(UIColor color)
        {
            
        }
    }
}