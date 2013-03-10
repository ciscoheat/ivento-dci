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
        #region Roles and RoleInterfaces

        // Ledgers is the only Role played in this Context.
        // Note that the Role Player is just inheriting another interface.
        internal LedgersRole Ledgers { get; set; }
        public interface LedgersRole : ICollection<LedgerEntry> {}

        #endregion

        #region Constructors and Role bindings

        public Account(LedgersRole ledgers)
        {
            BindRoles(ledgers);
        }

        private void BindRoles(object ledgers)
        {
            Ledgers = (LedgersRole)ledgers;
        }

        #endregion

        #region Interactions

        // Interactions must use one of the Context.Execute/ExecuteAndReturn overloads.

        // In this case, an Action since it's very simple. Remember to specify "this" 
        // as context in the second parameter.
        public void Deposit(decimal amount)
        {
            Context.Execute(() => Ledgers.AddEntry("Depositing", amount), this);
        }

        public void Withdraw(decimal amount)
        {
            Context.Execute(() => Ledgers.AddEntry("Withdrawing", -amount), this);
        }

        public decimal Balance
        {
            get { return Context.ExecuteAndReturn(() => Ledgers.Balance(), this); }
        }

        // This is just a get accessor for a Role, so it doesn't need Context.Execute.
        public IEnumerable<LedgerEntry> LedgersList
        {
            get { return Ledgers; }
        }

        #endregion
    }

    #region RoleMethods

    static class AccountRoleMethods
    {
        // Use case implementations of the Account.

        /// <summary>
        /// Changing the account amount means adding an entry to the Ledgers.
        /// </summary>
        public static void AddEntry(this Account.LedgersRole ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry { Message = message, Amount = amount });
        }

        /// <summary>
        /// The Account balance is the sum of all Ledger amounts.
        /// </summary>
        public static decimal Balance(this Account.LedgersRole ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }

    #endregion
}
