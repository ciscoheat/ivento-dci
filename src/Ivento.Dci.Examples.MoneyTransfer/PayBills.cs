using System;
using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
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
        internal SourceAccount Source { get; private set; }
        public interface SourceAccount
        {
            void Withdraw(decimal amount);
            decimal Balance { get; }
        }

        // Second role is the Creditors, a list of entities that will receive
        // payment from the Source Role. Using a simple interface inheritance
        // for the Role Contract.
        internal BillCreditors Creditors { get; private set; }
        public interface BillCreditors : IEnumerable<Creditor>
        {}

        #endregion

        #region RolePlayers

        // When a Methodful Role in a Context is executing another
        // context, the latter is said to be a nested Context.
        //
        // Since the Roles are internal to the Context, the Methodful Role 
        // must access the objects originally bound to the Context in the 
        // BindRoles method to be able to create the nested Context object.
        //
        // Therefore a Data Transfer Object called RolePlayers are created.
        // It will be instantiated in the BindRoles method.

        public PayBillsRolePlayers RolePlayers { get; private set; }
        public sealed class PayBillsRolePlayers
        {
            // Using the same properties as sent to the BindRoles method.

            public Account Source { get; set; }
            public IEnumerable<Creditor> Creditors { get; set; }

            // It should not be exposed to the outside world.
            internal PayBillsRolePlayers() {}
        }

        #endregion

        #region Constructors and Role bindings

        // The BindRoles is not only binding now, but also creating the 
        // PayBillsRolePlayers object so it can be used for creating
        // nested contexts.

        public PayBills(Account source, IEnumerable<Creditor> creditors)
        {
            BindRoles(source, creditors);
        }

        private void BindRoles(Account source, IEnumerable<Creditor> creditors)
        {
            RolePlayers = new PayBillsRolePlayers { Source = source, Creditors = creditors };

            Source = source.ActLike<SourceAccount>();
            Creditors = creditors.ActLike<BillCreditors>();
        }

        #endregion

        #region Context members

        public void Execute()
        {
            // As always, executing the Methodful Role with the static
            // Context.Execute, supplying "this" as second argument
            // for setting the context.
            Context.Execute(Source.PayBills, this);
        }

        #endregion
    }

    #region Methodful Roles

    static class PayBillsMethodfulRoles
    {
        /// <summary>
        /// Use case implementation of PayBills.
        /// </summary>
        public static void PayBills(this PayBills.SourceAccount source)
        {
            // Get the current Context first.
            var context = Context.Current<PayBills>(source, c => c.Source);

            // Get the current Creditor list, enumerating it to a list so it's not modified.
            var creditors = context.Creditors.ToList();

            // If not enough money to pay all creditors, don't pay anything.
            var surplus = source.Balance - creditors.Sum(c => c.AmountOwed);
            if (surplus < 0)
            {
                throw new AccountException(
                    string.Format("Not enough money on account to pay all bills.\r\n{0:c} more is needed.", Math.Abs(surplus)));
            }

            creditors.ForEach(creditor =>
            {
                // For each creditor, create a MoneyTransfer Context using data from
                // context.RolePlayers and the creditor role.
                var transferContext = new MoneyTransfer(context.RolePlayers.Source, creditor.Account, creditor.AmountOwed);

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
