namespace Ivento.Dci.Examples.MoneyTransfer
{
    class MoneyTransfer
    {
        public SourceAccount Source { get; private set; }
        public DestinationAccount Destination { get; private set; }
        public decimal Amount { get; private set; }

        public MoneyTransfer(SourceAccount source, DestinationAccount destination, decimal amount)
        {
            Source = source;
            Destination = destination;
            Amount = amount;
        }

        public void Execute()
        {
            Source.Transfer();
        }
    }
}
