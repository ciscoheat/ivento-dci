namespace Ivento.Dci.Examples.MoneyTransfer
{
    /// <summary>
    /// MoneyTransfer Context - Transferring money between two accounts.
    /// </summary>
    public sealed class MoneyTransfer
    {
        #region Roles and Role Contracts        

        // Roles are an identifier within the Context.
        // The first role of the MoneyTransfer Context is Source.
        // It's important to understand that a Role only exists 
        // inside its Context, and should not be confused with its type.
        // Unfortunately they cannot be private because they are used in 
        // extension methods.
        //                     vvvvvv
        internal SourceAccount Source { get; set; }

        // A Role usually has a Role Contract, which is an interface
        // that the object playing this Role must fulfill.
        // In this Context, the Source Role must be able to Withdraw 
        // money from itself.
        public interface SourceAccount
        {
            void Withdraw(decimal amount);
        }

        // The second role of the MoneyTransfer Context is Destination.
        // Same as for the Source role, if you reason about Roles you
        // speak only about their names, not their types.
        //                          vvvvvvvvvvv
        internal DestinationAccount Destination { get; set; }

        // Role Contract of the Destination Role.
        // In this Context, the Destination Role must be able to Deposit 
        // money to itself.
        public interface DestinationAccount
        {
            void Deposit(decimal amount);
        }

        // The third and final role of the MoneyTransfer Context: Amount.
        // It has no Role Contract because it's a built-in type with no interface.
        internal decimal Amount { get; set; }

        #endregion

        #region RolePlayers

        // There are no nested contexts in this Context (or its methodful Roles)
        // So there is no need to create a RolePlayers object for storing the
        // RolePlayers supplied to the Context constructor.
        //
        // PayBills on the other hand is using a nested Context, so it needs to
        // store the RolePlayers for usage when creating the nested Context.
        // See the PayBills Context for more information.

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
            Source = source.ActLike<SourceAccount>();
            Destination = destination.ActLike<DestinationAccount>();
            Amount = amount;
        }

        #endregion

        #region Execution

        /// <summary>
        /// This method executes the Context/use case.
        /// </summary>
        public void Execute()
        {
            // Kick off the use case by calling the methodful Role "Transfer" on the 
            // Source Role to make the money transfer. See the 
            // "MoneyTransferMethodfulRoles" class below for details about Methodful Roles.

            // Note that it must be invoked using the static Context.Execute method, 
            // supplying itself as context. This is a limitation of using extension methods.
            Context.Execute(Source.Transfer, this);
        }

        #endregion
    }

    #region Methodful Roles
    
    // Methodful Roles are the actual use case implementation, a translation from use case steps
    // to an algorithm. For this MoneyTransfer case, the use case is very simple and will only
    // have one Methodful Role, "Transfer".
    static class MoneyTransferMethodfulRoles
    {
        public static void Transfer(this MoneyTransfer.SourceAccount source)
        {
            // First get a reference to the context, MoneyTransfer.
            // The two parameters is a sanity check that the extension parameter for 
            // the Role "source" is actually the same as the Context property.
            //
            // Using this overload of Context.Current, you can be sure that "source"
            // and "context.Source" is the same object. Unless you make a big mistake 
            // in the Context class that is nearly always the case though, so it can 
            // be called without parameters for a simpler syntax.
            var context = Context.Current<MoneyTransfer>(source, c => c.Source);

            // Now when we have the context available, it's time for action.
            // The transfer is very simplified here, it just deposits money in the
            // destination account, and withdraws it from the source account.
            // A real scenario will probably use logging and database transactions.

            // Note the simplicity and readability of the code. It shows exactly
            // what should happen. Reasoning about this part of the code is easy 
            // and gives a nice view how the system works.

            context.Destination.Deposit(context.Amount);
            context.Source.Withdraw(context.Amount);
        }
    }

    #endregion
}
