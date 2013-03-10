namespace Ivento.Dci.Templates
{
    // The class Ivento.Dci.Examples.MoneyTransfer.MoneyTransfer 
    // gives a detailed introduction to Context objects.

    /// <summary>
    /// A starting point for a DCI Context.
    /// </summary>
    public sealed class ContextTemplate
    {
        #region Roles and RoleInterfaces

        // Note the naming convention: "NAME" for the identifier, "NAMERole" for the RoleInterface.

        internal NameRole Name { get; private set; }
        public interface NameRole
        {
            decimal SomeProperty { get; }
        }

        // Role: AnotherName, RoleInterface: AnotherNameRole
        internal AnotherNameRole AnotherName { get; private set; }
        public interface AnotherNameRole
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
            Name = (NameRole)roleName;
            AnotherName = (AnotherNameRole)anotherRoleName;
        }

        #endregion

        #region Interactions

        public void Execute()
        {
            // Execute the RoleMethod with the static Context.Execute, supplying 
            // "this" as second argument for setting the context.
            Context.Execute(Name.DoSomething, this);
        }

        #endregion
    }

    #region RoleMethods

    static class ContextTemplateRoleMethods
    {
        /// <summary>
        /// Use case implementation of ContextTemplate.
        /// </summary>
        public static void DoSomething(this ContextTemplate.NameRole nameRoleName)
        {
            // Get the current Context. Optional arguments to make sure that the 
            // underlying object is the same as the Context Role.
            var context = Context.Current<ContextTemplate>(nameRoleName, c => c.Name);

            // Finally, implement the use case with the Context Roles.

            if(nameRoleName.SomeProperty > 0)
                context.AnotherName.SomeMethod();
        }
    }

    #endregion
}
