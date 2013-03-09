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

        // Note the naming convention: "ROLE" for the identifier, "ROLERole" for the Role Contract.

        // In this case: RoleName and RoleNameRole
        internal RoleNameRole RoleName { get; private set; }
        public interface RoleNameRole
        {
            decimal SomeProperty { get; }
        }

        // Role name: AnotherRoleName, Contract type: AnotherRoleNameRole
        internal AnotherRoleNameRole AnotherRoleName { get; private set; }
        public interface AnotherRoleNameRole
        {
            void SomeMethod();
        }

        #endregion

        #region Constructors and Role bindings

        public ContextTemplate(object roleName, object anotherRoleName)
        {
            BindRoles(roleName, anotherRoleName);
        }

        private void BindRoles(object roleName, object anotherRoleName)
        {
            // Bind RolePlayers to Roles
            RoleName = (RoleNameRole)roleName;
            AnotherRoleName = (AnotherRoleNameRole)anotherRoleName;
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
        public static void DoSomething(this ContextTemplate.RoleNameRole roleName)
        {
            // Get the current Context. Optional arguments to make sure that the 
            // underlying object is the same as the Context Role.
            var context = Context.Current<ContextTemplate>(roleName, c => c.RoleName);

            // Finally, implement the use case with the Context Roles.

            if(roleName.SomeProperty > 0)
                context.AnotherRoleName.SomeMethod();
        }
    }

    #endregion
}
