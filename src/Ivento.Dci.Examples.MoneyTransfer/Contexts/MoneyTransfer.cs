namespace Ivento.Dci.Examples.MoneyTransfer.Contexts
{
    /// <summary>
    /// MoneyTransfer Context - Transferring money between two accounts.
    /// </summary>
    public sealed class MoneyTransfer
    {
        #region Roles and RoleInterfaces

        // Roles are an identifier within the Context.
        // The first role of the MoneyTransfer Context is SourceAccount.
        // It's important to understand that a Role only exists 
        // inside its Context, and should not be confused with its type.
        // Roles should be private to the Context but cannot in C#, because 
        // they are used in extension methods.
        //                         vvvvvvvvvvvvv
        internal SourceAccountRole SourceAccount { get; set; }

        // A Role has a RoleInterface, which is an interface
        // that the object playing this Role must fulfill.
        // In this Context, the SourceAccount Role must be able to Withdraw 
        // money from itself.
        public interface SourceAccountRole
        {
            void Withdraw(decimal amount);
        }

        // Also note the naming convention for roles: If the Role name is "NAME",
        // the RoleInterface type is "NAMERole".

        // The second Role of the MoneyTransfer Context is DestinationAccount.
        // Same as for the SourceAccount Role, if you reason about Roles you
        // speak only about their names, not their types. This concept is more 
        // obvious in the Transfer method of the source account below.
        //                              vvvvvvvvvvvvvvvvvv
        internal DestinationAccountRole DestinationAccount { get; set; }

        // RoleInterface of the DestinationAccount Role.
        // In this Context, the DestinationAccount Role must be able to Deposit 
        // money to itself.
        public interface DestinationAccountRole
        {
            void Deposit(decimal amount);
        }

        // The third and final role of the MoneyTransfer Context: Amount.
        // It has no RoleInterface because it's a simple built-in type.
        internal decimal Amount { get; set; }

        #endregion

        #region Constructors and Role bindings

        // When the Context is created, the Roles will be bound using the supplied arguments.
        // There can be multiple constructors with different ways of retreiving the objects
        // needed for the binding (database lookup, web service, etc), but there can only be 
        // one BindRoles method, which does the final Role binding.

        // The constructor(s) should be strongly typed to check for errors.

        public MoneyTransfer(PayBills.SourceAccountRole source, Account destination, decimal amount)
        {
            BindRoles(source, destination, amount);
        }

        public MoneyTransfer(SourceAccountRole source, DestinationAccountRole destination, decimal amount)
        {
            BindRoles(source, destination, amount);
        }

        // The BindRoles method however, should use object so anything can be sent here
        // from the constructors, then casted to the RoleInterface.

        private void BindRoles(object source, object destination, decimal amount)
        {
            // Make the RolePlayers act the Roles they are supposed to.
            SourceAccount = (SourceAccountRole)source;
            DestinationAccount = (DestinationAccountRole)destination;
            Amount = amount;
        }

        #endregion

        #region Interactions

        /// <summary>
        /// This method executes the Context/use case.
        /// </summary>
        public void Execute()
        {
            // Kick off the use case by calling the RoleMethod "Transfer" on the 
            // SourceAccount Role to make the money transfer. See the 
            // "MoneyTransferRoleMethods" class below for details about RoleMethods.

            // Note that it must be invoked using the static Context.Execute method, 
            // supplying itself as context. This is a limitation of using extension methods.
            Context.Execute(SourceAccount.Transfer, this);
        }

        #endregion
    }

    #region RoleMethods
    
    // RoleMethods are the actual use case implementation, a translation from use case steps
    // to an algorithm. For this MoneyTransfer case, the use case is very simple and will only
    // have one RoleMethod, "Transfer".
    static class MoneyTransferRoleMethods
    {
        public static void Transfer(this MoneyTransfer.SourceAccountRole sourceAccount)
        {
            // First get a reference to the context, MoneyTransfer.
            // The two parameters are a sanity check that the extension parameter for 
            // the Role "sourceAccount" is actually the same as the Context property.

            // Using this overload of Context.Current, you can be sure that "sourceAccount"
            // and "context.SourceAccount" is the same object. Unless you make a big mistake 
            // in the Context class that is nearly always the case though, so it can 
            // be called without parameters for a simpler syntax.
            var c = Context.Current<MoneyTransfer>(sourceAccount, ctx => ctx.SourceAccount);

            // Now when we have the context available, it's time for action.
            // The transfer is very simplified here, it just deposits money in the
            // destination account, and withdraws it from the sourceAccount account.
            // A real scenario will probably use logging and database transactions.

            // Note the simplicity and readability of the code. It shows exactly
            // what should happen, and will look very similar to the Use Case description. 

            // Reasoning about this part of the code is easy and gives a nice view how the system works.

            c.DestinationAccount.Deposit(c.Amount);
            c.SourceAccount.Withdraw(c.Amount);
        }
    }

    #endregion
}
