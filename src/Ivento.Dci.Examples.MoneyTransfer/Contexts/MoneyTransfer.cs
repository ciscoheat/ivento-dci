namespace Ivento.Dci.Examples.MoneyTransfer.Contexts
{
    /// <summary>
    /// MoneyTransfer Context - Transferring money between two accounts.
    /// </summary>
    public sealed class MoneyTransfer
    {
        #region Roles and Role Contracts        

        // Roles are an identifier within the Context.
        // The first role of the MoneyTransfer Context is SourceAccount.
        // It's important to understand that a Role only exists 
        // inside its Context, and should not be confused with its type.
        // Unfortunately they cannot be private because they are used in 
        // extension methods.
        //                         vvvvvvvvvvvvv
        internal SourceAccountRole SourceAccount { get; set; }

        // A Role usually has a Role Contract, which is an interface
        // that the object playing this Role must fulfill.
        // In this Context, the SourceAccount Role must be able to Withdraw 
        // money from itself.
        public interface SourceAccountRole
        {
            void Withdraw(decimal amount);
        }

        // The second role of the MoneyTransfer Context is DestinationAccount.
        // Same as for the SourceAccount role, if you reason about Roles you
        // speak only about their names, not their types.
        //                              vvvvvvvvvvvvvvvvvv
        internal DestinationAccountRole DestinationAccount { get; set; }

        // Role Contract of the DestinationAccount Role.
        // In this Context, the DestinationAccount Role must be able to Deposit 
        // money to itself.
        public interface DestinationAccountRole
        {
            void Deposit(decimal amount);
        }

        // The third and final role of the MoneyTransfer Context: Amount.
        // It has no Role Contract because it's a built-in type with no interface.
        internal decimal Amount { get; set; }

        #endregion

        #region Constructors and Role bindings

        // When the Context is created, the Roles will be bound using the supplied arguments.
        // There can be multiple constructors with different ways of retreiving the objects
        // needed for the binding (database lookup, web service, etc), but there can only be 
        // one BindRoles method, which does the final Role binding.
        public MoneyTransfer(Account source, Account destination, decimal amount)
        {
            // All the roles are of the correct type for binding, so do it right away.
            BindRoles(source, destination, amount);
        }

        private void BindRoles(Account source, Account destination, decimal amount)
        {
            // Make the RolePlayers act the Roles they are supposed to.
            // Using the ActLike<T> extension method, objects will behave like an interface without 
            // implementing it. This avoids the classes outside the context getting polluted with 
            // interface implementations of the Role Contracts.
            SourceAccount = source.ActLike<SourceAccountRole>();
            DestinationAccount = destination.ActLike<DestinationAccountRole>();
            Amount = amount;
        }

        #endregion

        #region Context members

        /// <summary>
        /// This method executes the Context/use case.
        /// </summary>
        public void Execute()
        {
            // Kick off the use case by calling the methodful Role "Transfer" on the 
            // SourceAccount Role to make the money transfer. See the 
            // "MoneyTransferMethodfulRoles" class below for details about Methodful Roles.

            // Note that it must be invoked using the static Context.Execute method, 
            // supplying itself as context. This is a limitation of using extension methods.
            Context.Execute(SourceAccount.Transfer, this);
        }

        #endregion
    }

    #region Methodful Roles
    
    // Methodful Roles are the actual use case implementation, a translation from use case steps
    // to an algorithm. For this MoneyTransfer case, the use case is very simple and will only
    // have one Methodful Role, "Transfer".
    static class MoneyTransferMethodfulRoles
    {
        public static void Transfer(this MoneyTransfer.SourceAccountRole sourceAccount)
        {
            // First get a reference to the context, MoneyTransfer.
            // The two parameters is a sanity check that the extension parameter for 
            // the Role "sourceAccount" is actually the same as the Context property.
            //
            // Using this overload of Context.Current, you can be sure that "sourceAccount"
            // and "context.SourceAccount" is the same object. Unless you make a big mistake 
            // in the Context class that is nearly always the case though, so it can 
            // be called without parameters for a simpler syntax.
            var context = Context.Current<MoneyTransfer>(sourceAccount, c => c.SourceAccount);

            // Now when we have the context available, it's time for action.
            // The transfer is very simplified here, it just deposits money in the
            // destination account, and withdraws it from the sourceAccount account.
            // A real scenario will probably use logging and database transactions.

            // Note the simplicity and readability of the code. It shows exactly
            // what should happen. Reasoning about this part of the code is easy 
            // and gives a nice view how the system works.

            context.DestinationAccount.Deposit(context.Amount);
            context.SourceAccount.Withdraw(context.Amount);
        }
    }

    #endregion
}
