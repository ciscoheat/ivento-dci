using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;
using ImpromptuInterface;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class Account
    {
        public Ledgers Ledgers { get; set; }

        public Account(ICollection<LedgerEntry> ledgers)
        {
            Ledgers = ledgers.ActLike<Ledgers>();
        }

        public decimal Balance
        {
            get { return Ledgers.Balance(); }
        }

        public void Deposit(decimal amount)
        {
            Ledgers.AddEntry("Depositing", amount);
        }

        public void Withdraw(decimal amount)
        {
            Ledgers.AddEntry("Withdrawing", -amount);
        }
    }

    public interface SourceAccount
    {
        void Withdraw(decimal amount);
    }

    public interface DestinationAccount
    {
        void Deposit(decimal amount);
    }

    public static class SourceAccountTraits
    {
        public static void Transfer(this SourceAccount source)
        {
            var context = Context.CurrentAs<MoneyTransfer>();

            context.Destination.Deposit(context.Amount);
            source.Withdraw(context.Amount);
        }

        public static void PayBills(this SourceAccount source)
        {
            var context = Context.CurrentAs<PayBills>();
            var creditors = context.Creditors.ToList();

            creditors.ForEach(c =>
                                  {
                                      var transferContext = new MoneyTransfer(source, c.Account, c.AmountOwed);
                                      Context.Execute(transferContext);
                                  });
        }
    }

    public interface Ledgers : ICollection<LedgerEntry>
    {}

    public static class LedgersTraits
    {
        public static void AddEntry(this Ledgers ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry(message, amount));
        }

        public static decimal Balance(this Ledgers ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }
}
