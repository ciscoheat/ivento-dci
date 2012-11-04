using ImpromptuInterface;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class MoneyTransfer
    {
        public Account Source { get; private set; }
        public Account Destination { get; private set; }
        public decimal Amount { get; private set; }

        public MoneyTransfer(Account source, Account destination, decimal amount)
        {
            Source = source;
            Destination = destination;
            Amount = amount;
        }

        public void Execute()
        {
            Source.ActLike<SourceAccount>().Transfer();
        }

        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }

        public interface DestinationAccount
        {
            void Deposit(decimal amount);
        }
    }

    static class MoneyTransferMethodfulRoles
    {
        public static void Transfer(this MoneyTransfer.SourceAccount source)
        {
            var context = Context.Current<MoneyTransfer>(source, c => c.Source);

            context.Destination.Deposit(context.Amount);
            source.Withdraw(context.Amount);
        }
    }
}
