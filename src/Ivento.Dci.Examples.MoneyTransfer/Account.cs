using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;
using ImpromptuInterface;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class Account
    {
        public AccountLedgers Ledgers { get; set; }

        public Account(ICollection<LedgerEntry> ledgers)
        {
            Ledgers = ledgers.ActLike<AccountLedgers>();
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

        public interface AccountLedgers : ICollection<LedgerEntry>
        {}
    }

    internal static class AccountLedgersTraits
    {
        public static void AddEntry(this Account.AccountLedgers ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry { Message = message, Amount = amount });
        }

        public static decimal Balance(this Account.AccountLedgers ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }
}
