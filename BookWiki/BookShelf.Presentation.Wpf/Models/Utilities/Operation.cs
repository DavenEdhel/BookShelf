namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public class Operation : IOperation
    {
        private readonly Action _operation;

        public Operation(Action operation)
        {
            this._operation = operation;
        }

        public void Execute()
        {
            this._operation();
        }
    }
}