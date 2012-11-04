using System.Collections.Generic;
using System.Linq;
using ImpromptuInterface;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class PayBills
    {
        public Account Source { get; private set; }
        public IEnumerable<Creditor> Creditors { get; private set; }

        public PayBills(Account source, IEnumerable<Creditor> creditors)
        {
            Source = source;
            Creditors = creditors;
        }

        public void Execute()
        {
            Source.ActLike<SourceAccount>().PayBills();
        }

        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }
    }

    static class PayBillsMethodfulRoles
    {
        public static void PayBills(this PayBills.SourceAccount source)
        {
            var context = Context.Current<PayBills>(source, c => c.Source);
            var creditors = context.Creditors.ToList();

            creditors.ForEach(creditor =>
            {
                var transferContext = new MoneyTransfer(context.Source, creditor.Account, creditor.AmountOwed);
                Context.Execute(transferContext);
            });
        }
    }
}
