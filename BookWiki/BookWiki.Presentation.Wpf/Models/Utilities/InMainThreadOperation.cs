namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    public class InMainThreadOperation : IOperation
    {
        private readonly IOperation _operation;
        private readonly IThreadInvoker _threadInvoker;

        public InMainThreadOperation(IOperation operation, IThreadInvoker threadInvoker)
        {
            this._operation = operation;
            this._threadInvoker = threadInvoker;
        }

        public void Execute()
        {
            this._threadInvoker.InvokeInMainThread(this._operation.Execute);
        }
    }
}