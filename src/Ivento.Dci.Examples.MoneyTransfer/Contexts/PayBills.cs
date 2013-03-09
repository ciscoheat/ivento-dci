using System;
using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer.Contexts
{
    // If you haven't, see MoneyTransfer.cs for a more detailed introduction to Context objects.

    /// <summary>
    /// PayBills Context - Using an Account to pay bills to Creditors.
    /// </summary>
    public sealed class PayBills
    {
        #region Roles and Role Contracts

        // First role for PayBills - The Account that should pay the bills.
        // Its Role Contract declares that it must support Withdrawing money
        // and read the balance.
        internal SourceAccountRole SourceAccount { get; private set; }
        public interface SourceAccountRole
        {
            void Withdraw(decimal amount);
            decimal Balance { get; }
        }

        // Second role is the Creditors, a list of entities that will receive
        // payment from the SourceAccount Role. Using a simple interface inheritance
        // for the Role Contract.
        internal IEnumerable<Creditor> Creditors { get; private set; }

        #endregion

        #region Constructors and Role bindings

        public PayBills(SourceAccountRole source, IEnumerable<Creditor> creditors)
        {
            BindRoles(source, creditors);
        }

        private void BindRoles(object source, object creditors)
        {
            SourceAccount = (SourceAccountRole)source;
            Creditors = (IEnumerable<Creditor>)creditors;
        }

        #endregion

        #region Context members

        public void Execute()
        {
            // As always, executing the Methodful Role with the static
            // Context.Execute, supplying "this" as second argument
            // for setting the context.
            Context.Execute(SourceAccount.PayBills, this);
        }

        #endregion
    }

    #region Methodful Roles

    static class PayBillsMethodfulRoles
    {
        /// <summary>
        /// Use case implementation of PayBills.
        /// </summary>
        public static void PayBills(this PayBills.SourceAccountRole sourceAccount)
        {
            // Get the current Context first.
            var c = Context.Current<PayBills>(sourceAccount, ct => ct.SourceAccount);

            // Get the current Creditor list, enumerating it to a list so it's not modified.
            var creditors = c.Creditors.ToList();

            // If not enough money to pay all creditors, don't pay anything.
            var surplus = sourceAccount.Balance - creditors.Sum(cr => cr.AmountOwed);
            if (surplus < 0)
            {
                throw new AccountException(
                    string.Format("Not enough money on account to pay all bills.\r\n{0:c} more is needed.", Math.Abs(surplus)));
            }

            creditors.ForEach(creditor =>
            {
                // For each creditor, create a MoneyTransfer Context. Because the Context requires an Account
                // object and sourceAccount is a SourceAccountRole, use the IsA<T> extension method
                // to get the original Account object.
                var transferContext = new MoneyTransfer(sourceAccount, creditor.Account, creditor.AmountOwed);

                // When the whole context object can be supplied to Context.Execute, there
                // is no need to set the context explicitly. Context.Execute will look for
                // a method called Execute and invoke it.
                //
                // If another method than Execute must be invoked, use the Action overload.
                Context.Execute(transferContext);
            });
        }
    }

    #endregion
}
