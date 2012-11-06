using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class PayBills
    {
        #region Roles and Role Contracts

        public SourceAccount Source { get; private set; }
        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }

        public BillCreditors Creditors { get; private set; }
        public interface BillCreditors : IEnumerable<Creditor>
        {}

        #endregion

        #region RolePlayers

        public PayBillsRolePlayers RolePlayers { get; private set; }
        public sealed class PayBillsRolePlayers
        {
            public Account Source { get; set; }
            public IEnumerable<Creditor> Creditors { get; set; }

            internal PayBillsRolePlayers() {}
        }

        #endregion

        #region Constructors and Role bindings

        public PayBills(Account source, IEnumerable<Creditor> creditors)
        {
            RolePlayers = new PayBillsRolePlayers { Source = source, Creditors = creditors };
            BindRoles();
        }

        private void BindRoles()
        {
            Source = RolePlayers.Source.ActLike<SourceAccount>();
            Creditors = RolePlayers.Creditors.ActLike<BillCreditors>();
        }

        #endregion

        #region Execution

        public void Execute()
        {
            Source.PayBills();
        }

        #endregion
    }

    #region Methodful Roles

    static class PayBillsMethodfulRoles
    {
        public static void PayBills(this PayBills.SourceAccount source)
        {
            var context = Context.Current<PayBills>(source, c => c.Source);
            var creditors = context.Creditors.ToList();

            creditors.ForEach(creditor =>
            {
                var transferContext = new MoneyTransfer(context.RolePlayers.Source, creditor.Account, creditor.AmountOwed);
                Context.Execute(transferContext);
            });
        }
    }

    #endregion
}
