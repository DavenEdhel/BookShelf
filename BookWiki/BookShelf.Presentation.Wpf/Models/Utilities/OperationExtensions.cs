namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    public static class OperationExtensions
    {
        public static FailSafeOperation FailSafe(this IOperation self)
        {
            return new FailSafeOperation(self);
        }

        public static InMainThreadOperation InMainThread(this IOperation self, IThreadInvoker threadInvoker = null)
        {
            return new InMainThreadOperation(self, threadInvoker.SelfOrResolve());
        }
    }
}