using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class PayBills
    {
        public PayBillsRolePlayers RolePlayers { get; private set; }
        public class PayBillsRolePlayers
        {
            public Account Source { get; set; }
            public IEnumerable<Creditor> Creditors { get; set; }
        }

        public SourceAccount Source { get; private set; }
        public BillCreditors Creditors { get; private set; }

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

        public void Execute()
        {
            Source.PayBills();
        }

        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }

        public interface BillCreditors : IEnumerable<Creditor>
        {}
    }

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
}
