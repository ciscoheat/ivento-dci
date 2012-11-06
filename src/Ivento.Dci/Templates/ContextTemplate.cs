namespace Ivento.Dci.Templates
{
    // The class Ivento.Dci.Examples.MoneyTransfer.MoneyTransfer 
    // gives a detailed introduction to Context objects.

    /// <summary>
    /// A starting point for a DCI Context.
    /// </summary>
    public sealed class ContextTemplate
    {
        #region Roles and Role Contracts

        internal RoleContract RoleName { get; private set; }
        public interface RoleContract
        {
            void RolePlayerMethod();
            decimal RolePlayerProperty { get; }
        }

        #endregion

        #region RolePlayers

        public ContextTemplateRolePlayers RolePlayers { get; private set; }
        public sealed class ContextTemplateRolePlayers
        {
            // Using the same properties as sent to the BindRoles method.
            public string Source { get; set; }

            // It should not be exposed to the outside world.
            internal ContextTemplateRolePlayers() {}
        }

        #endregion

        #region Constructors and Role bindings

        // The BindRoles is not only binding now, but also creating the 
        // PayBillsRolePlayers object so it can be used for creating
        // nested contexts.

        public ContextTemplate(string roleName)
        {
            BindRoles(roleName);
        }

        private void BindRoles(string roleName)
        {
            // Create RolePlayers
            RolePlayers = new ContextTemplateRolePlayers {Source = roleName};

            // Bind RolePlayers to Roles
            RoleName = roleName.ActLike<RoleContract>();
        }

        #endregion

        #region Context members

        public void Execute()
        {
            // Execute the Methodful Role with the static Context.Execute, supplying 
            // "this" as second argument for setting the context.
            Context.Execute(RoleName.DoSomething, this);
        }

        #endregion
    }

    #region Methodful Roles

    static class ContextTemplateMethodfulRoles
    {
        /// <summary>
        /// Use case implementation of ContextTemplate.
        /// </summary>
        public static void DoSomething(this ContextTemplate.RoleContract roleName)
        {
            // Get the current Context
            var context = Context.Current<ContextTemplate>(roleName, c => c.RoleName);

            // Implement the use case with the context roles.
            if(context.RoleName.RolePlayerProperty > 0)
                context.RoleName.RolePlayerMethod();
        }
    }

    #endregion
}
