namespace Ivento.Dci.Examples.MoneyTransfer
{
    public sealed class MoneyTransfer
    {
        #region Roles and Role Contracts
        
        public SourceAccount Source { get; private set; }
        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }

        public DestinationAccount Destination { get; private set; }
        public interface DestinationAccount
        {
            void Deposit(decimal amount);
        }

        public decimal Amount { get; private set; }

        #endregion

        #region RolePlayers

        public MoneyTransferRolePlayers RolePlayers { get; private set; }
        public sealed class MoneyTransferRolePlayers
        {
            public Account Source { get; set; }
            public Account Destination { get; set; }
            public decimal Amount { get; set; }
        }

        #endregion

        #region Constructors and Role bindings

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

        #endregion

        #region Execution

        public void Execute()
        {
            Source.Transfer();
        }

        #endregion
    }

    #region Methodful Roles

    static class MoneyTransferMethodfulRoles
    {
        public static void Transfer(this MoneyTransfer.SourceAccount source)
        {
            var context = Context.Current<MoneyTransfer>(source, c => c.Source);

            context.Destination.Deposit(context.Amount);
            context.Source.Withdraw(context.Amount);
        }
    }

    #endregion
}
