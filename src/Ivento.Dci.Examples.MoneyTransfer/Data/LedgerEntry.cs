namespace Ivento.Dci.Examples.MoneyTransfer.Data
{
    public class LedgerEntry
    {
        public string Message { get; set; }
        public decimal Amount { get; set; }

        public override string ToString()
        {
            return string.Format("{0,-25}{1:c}", Message, Amount);
        }
    }
}
