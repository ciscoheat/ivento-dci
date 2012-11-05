namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class MoneyTransfer
    {
        public MoneyTransferRolePlayers RolePlayers { get; private set; }
        public class MoneyTransferRolePlayers
        {
            public Account Source { get; set; }
            public Account Destination { get; set; }
            public decimal Amount { get; set; }
        }

        public SourceAccount Source { get; private set; }
        public DestinationAccount Destination { get; private set; }
        public decimal Amount { get; private set; }

        public MoneyTransfer(Account source, Account destination, decimal amount)
        {
            RolePlayers = new MoneyTransferRolePlayers { Source = source, Destination = destination, Amount = amount };
            BindRoles();
        }

        private void BindRoles()
        {
            Source = RolePlayers.Source.ActLike<SourceAccount>();
            Destination = RolePlayers.Destination.ActLike<DestinationAccount>();
            Amount = RolePlayers.Amount;
        }

        public void Execute()
        {
            Source.Transfer();
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
            context.Source.Withdraw(context.Amount);
        }
    }
}
