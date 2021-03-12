namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public class FailSafeOperation : IOperation
    {
        private readonly IOperation _operation;

        public FailSafeOperation(Action operation)
        {
            this._operation = new Operation(operation);
        }

        public FailSafeOperation(IOperation operation)
        {
            this._operation = operation;
        }

        public void Execute()
        {
            try
            {
                this._operation.Execute();
            }
            catch (Exception ex)
            {
            }
        }
    }
}