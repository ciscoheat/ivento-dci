using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer.Contexts
{
    // If you haven't, see MoneyTransfer.cs for a more detailed introduction to Context objects.

    /// <summary>
    /// Account Context.
    /// This Context doesn't have an Execute method, instead it implements
    /// methods required to play Roles in the MoneyTransfer and PayBills Context.
    /// (Deposit and Withdraw)
    /// </summary>
    public sealed class Account :
        MoneyTransfer.SourceAccountRole, MoneyTransfer.DestinationAccountRole,
        PayBills.SourceAccountRole
    {
        #region Roles and Role Contracts

        // Ledgers is the only Role played in this Context.
        // Note that the Role Player is just inheriting another interface.
        // Since no Methodful Roles are using the Ledgers role from a 
        // Context method, it can be private.

        private ICollection<LedgerEntry> Ledgers { get; set; }

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
            Ledgers = ledgers;
        }

        #endregion

        #region Context members

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
        public static void AddEntry(this ICollection<LedgerEntry> ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry { Message = message, Amount = amount });
        }

        /// <summary>
        /// The Account balance is the sum of all Ledger amounts.
        /// </summary>
        public static decimal Balance(this ICollection<LedgerEntry> ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }

    #endregion
}
