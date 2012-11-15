using System;
using System.Collections.Generic;
using Ivento.Dci.Abstract;

namespace Ivento.Dci
{
    /// <summary>
    /// Context accessor and executing environment.
    /// </summary>
    public sealed class Context
    {
        // Thread-safe singleton
        private static readonly Lazy<DciContext> LazyContext = new Lazy<DciContext>(() => new DciContext());
        public static DciContext CurrentContext { get { return LazyContext.Value; } }

        /// <summary>
        /// Return the current Context, ensuring that an object is the same as a Context property.
        /// </summary>
        /// <typeparam name="T">Context type</typeparam>
        /// <returns>Context in initialized scope</returns>
        public static T Current<T>() where T : class
        {
            return CurrentContext.Current<T>();
        }

        public static DciContext.ContextInitialization Initialize
        {
            get { return CurrentContext.Initialize; }
        }

        /// <summary>
        /// Return the current Context, ensuring that an object is the same as a Context property.
        /// </summary>
        /// <typeparam name="T">Context type</typeparam>
        /// <param name="role">Object playing a role in an extension method</param>
        /// <param name="contextRole">Context property that should be the same as the object</param>
        /// <returns>Context in initialized scope</returns>
        /// <exception cref="InvalidOperationException">If the object isn't equal to the specified Context property.</exception>
        public static T Current<T>(object role, Func<T, object> contextRole) where T : class
        {
            return CurrentContext.Current(role, contextRole);
        }

        #region Execution

        public static void Execute(Action action)
        {
            CurrentContext.Execute(action, null);
        }

        public static void Execute(Action action, object context)
        {
            // Need to specify context explicitly since the Action method is wrapped in a Func delegate.
            CurrentContext.ExecuteAndReturn(() => { action(); return true; }, context ?? action.Target);
        }

        public static T ExecuteAndReturn<T>(Func<T> func)
        {
            return CurrentContext.ExecuteAndReturn(func, null);
        }

        public static T ExecuteAndReturn<T>(Func<T> action, object context)
        {
            return CurrentContext.ExecuteAndReturn(action, context);
        }

        public static T ExecuteAndReturn<T>(object context)
        {
            return CurrentContext.ExecuteAndReturn<T>(context);
        }

        public static void Execute(object context)
        {
            CurrentContext.Execute(context);
        }

        #endregion

        #region Dependency Injection

        public static T Resolve<T>()
        {
            return CurrentContext.DependencyResolver.GetService<T>();
        }

        public static object Resolve(Type type)
        {
            return CurrentContext.DependencyResolver.GetService(type);
        }

        public static IEnumerable<T> ResolveAll<T>()
        {
            return CurrentContext.DependencyResolver.GetServices<T>();
        }

        public static IEnumerable<object> ResolveAll(Type type)
        {
            return CurrentContext.DependencyResolver.GetServices(type);
        }

        #endregion
    }
}
