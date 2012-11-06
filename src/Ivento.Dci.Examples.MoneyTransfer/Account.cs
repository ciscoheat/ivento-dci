using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class Account
    {
        #region Roles and Role Contracts

        public AccountLedgers Ledgers { get; private set; }
        public interface AccountLedgers : ICollection<LedgerEntry>
        {}

        #endregion

        #region RolePlayers

        public AccountRolePlayers RolePlayers { get; private set; }
        public class AccountRolePlayers
        {
            public ICollection<LedgerEntry> Ledgers { get; set; }
        }

        #endregion

        #region Constructors and Role bindings

        public Account(ICollection<LedgerEntry> ledgers)
        {
            RolePlayers = new AccountRolePlayers { Ledgers = ledgers };
            BindRoles();
        }

        private void BindRoles()
        {
            Ledgers = RolePlayers.Ledgers.ActLike<AccountLedgers>();
        }

        #endregion

        #region Context fields

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

        #endregion
    }

    #region Methodful Roles

    static class AccountMethodfulRoles
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

    #endregion
}
