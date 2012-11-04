using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class Account
    {
        public ICollection<LedgerEntry> Ledgers { get; private set; }
        private readonly AccountLedgers _ledgers;

        public Account(ICollection<LedgerEntry> ledgers)
        {
            Ledgers = ledgers;
            _ledgers = Ledgers.ActLike<AccountLedgers>();
        }

        public decimal Balance
        {
            get { return _ledgers.Balance(); }
        }

        public void Deposit(decimal amount)
        {
            _ledgers.AddEntry("Depositing", amount);
        }

        public void Withdraw(decimal amount)
        {
            _ledgers.AddEntry("Withdrawing", -amount);
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
