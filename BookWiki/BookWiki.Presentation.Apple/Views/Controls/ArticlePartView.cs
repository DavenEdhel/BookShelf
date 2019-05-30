using System;
using BookWiki.Core;
using BookWiki.Core.Findings;
using BookWiki.Presentation.Apple.Extentions;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class ArticlePartView : View
    {
        private readonly IFinding _finding;

        private UILabel _part;

        public ArticlePartView(IFinding finding)
        {
            _finding = finding;

            Initialize();
        }

        private void Initialize()
        {
            UserInteractionEnabled = false;

            _part = new UILabel();
            _part.UserInteractionEnabled = false;
            _part.Font = UIFont.SystemFontOfSize(20);
            _part.AttributedText = GetContent();
            _part.Lines = 0;
            Add(_part);

            Layout = () =>
            {
                _part.SetSizeThatFits(Frame.Width);
                _part.ChangePosition(0, 0);
            };

            Layout();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return _part.SizeThatFits(size);
        }

        private NSMutableAttributedString GetContent()
        {
            try
            {
                var result = new NSMutableAttributedString(_finding.Context.PlainText);

                result.AddAttribute(UIStringAttributeKey.Font, UIFont.BoldSystemFontOfSize(20), new NSRange(_finding.Result.Offset, _finding.Result.Length));

                var paragraphStyle = new NSMutableParagraphStyle();
                paragraphStyle.ParagraphSpacingBefore = 3;
                paragraphStyle.ParagraphSpacing = 3;
                paragraphStyle.FirstLineHeadIndent = 20;
                result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraphStyle, new NSRange(0, _finding.Result.Length));

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}