using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    // If you haven't, see MoneyTransfer.cs for a more detailed introduction to Context objects.

    /// <summary>
    /// Account Context.
    /// This Context doesn't have an Execute method, instead it implements
    /// methods required to play Roles in the MoneyTransfer and PayBills Context.
    /// (Deposit and Withdraw)
    /// </summary>
    public class Account
    {
        #region Roles and Role Contracts

        // Ledgers is the only Role played in this Context.
        // Note that the Role Player is just inheriting another interface.
        // Since no Methodful Roles are using the Ledgers role from a 
        // Context method, it can be private.

        private AccountLedgers Ledgers { get; set; }
        public interface AccountLedgers : ICollection<LedgerEntry>
        {}

        #endregion

        #region RolePlayers

        // As with the MoneyTransfer Context, no nested Contexts are used so
        // there is no need for accessing the original RolePlayer objects.
        // See PayBills for an example of RolePlayers and nested Contexts.

        #endregion

        #region Constructors and Role bindings

        // A simple binding, only making the ledgers act like the
        // AccountLedgers Role Contract.

        public Account(ICollection<LedgerEntry> ledgers)
        {
            BindRoles(ledgers);
        }

        private void BindRoles(ICollection<LedgerEntry> ledgers)
        {
            Ledgers = ledgers.ActLike<AccountLedgers>();
        }

        #endregion

        #region Context fields

        // Theoretically, these methods and properties should be invoked with the 
        // static Context.Execute() for a true Context execution. But since they 
        // are only using Roles within their own Context, and so are the 
        // Methodful Roles, it's not needed.

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

        public IEnumerable<LedgerEntry> LedgersList
        {
            get { return Ledgers; }
        }

        #endregion
    }

    #region Methodful Roles

    static class AccountMethodfulRoles
    {
        // Use case implementations of the Account.

        /// <summary>
        /// Changing the account amount means adding an entry to the Ledgers.
        /// </summary>
        public static void AddEntry(this Account.AccountLedgers ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry { Message = message, Amount = amount });
        }

        /// <summary>
        /// The Account balance is the sum of all Ledger amounts.
        /// </summary>
        public static decimal Balance(this Account.AccountLedgers ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }

    #endregion
}
