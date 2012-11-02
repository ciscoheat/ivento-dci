namespace Ivento.Dci.Examples.MoneyTransfer.Data
{
    public class Creditor
    {
        public string Name { get; set; }
        public decimal AmountOwed { get; set; }
        public DestinationAccount Account { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1:c}", Name, AmountOwed);
        }
    }
}
