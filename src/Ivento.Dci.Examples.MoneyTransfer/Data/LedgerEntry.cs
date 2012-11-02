namespace Ivento.Dci.Examples.MoneyTransfer.Data
{
    public class LedgerEntry
    {
        public string Message { get; private set; }
        public decimal Amount { get; private set; }

        public LedgerEntry(string message, decimal amount)
        {
            Message = message;
            Amount = amount;
        }
    }
}
