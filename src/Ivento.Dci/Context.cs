using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace Ivento.Dci
{
    public sealed class Context
    {
        private static Func<Stack> _stackAccessor;

        private static readonly Lazy<ContextInitialization> InitializeLazy = new Lazy<ContextInitialization>(() => new ContextInitialization());
        public static ContextInitialization Initialize { get { return InitializeLazy.Value; } }

        /// <summary>
        /// Return the current Context, ensuring that an object is the same as a Context property.
        /// </summary>
        /// <typeparam name="T">Context type</typeparam>
        /// <returns>Context in initialized scope</returns>
        public static T Current<T>() where T : class
        {
            var currentContext = _stackAccessor().Peek();

            if (currentContext.GetType() != typeof(T))
            {
                throw new InvalidOperationException(
                    string.Format("Context type mismatch: Expected {0}, was {1}", typeof (T), currentContext.GetType()));
            }

            return currentContext as T;
        }

        /// <summary>
        /// Return the current Context, ensuring that an object is the same as a Context property.
        /// </summary>
        /// <typeparam name="T">Context type</typeparam>
        /// <param name="role">Object playing a role in an extension method</param>
        /// <param name="contextRole">Context property that should be the same as the object</param>
        /// <returns>Context in initialized scope</returns>
        /// <exception cref="InvalidOperationException">If the object isn't equal to the specified Context property.</exception>
        /// <remarks>
        /// Sometimes the underlying RolePlayer must be accessed instead of the Role (interface) in the extension method,
        /// for example when creating nested contexts. This overload ensures that those objects are one and the same.
        /// </remarks>
        public static T Current<T>(object role, Func<T, object> contextRole) where T : class
        {
            var currentContext = Current<T>();

            if(!role.Equals(contextRole(currentContext)))
                throw new InvalidOperationException("Role is not bound to Context properly.");

            return currentContext;
        }

        #region Initialization

        public sealed class ContextInitialization
        {
            private static readonly Lazy<Stack> StaticStack = new Lazy<Stack>(() => new Stack());

            private static readonly Lazy<ThreadLocal<Stack>> ThreadStaticStack = new Lazy<ThreadLocal<Stack>>(
                () => new ThreadLocal<Stack>(
                    () => new Stack()
                )
            );

            internal ContextInitialization()
            {}

            /// <summary>
            /// Initialize Context as static for the whole application.
            /// </summary>
            public void InStaticScope()
            {
                With(() => StaticStack.Value);
            }

            /// <summary>
            /// Initialize Context as thread static (one Context per Thread)
            /// </summary>
            public void InThreadScope()
            {
                With(() => ThreadStaticStack.Value.Value);
            }

            /// <summary>
            /// Custom Initialization should call this method to finalize initialization.
            /// </summary>
            /// <param name="stackAccessor">A method returning a Stack that will be used as Context.</param>
            public void With(Func<Stack> stackAccessor)
            {
                if (_stackAccessor != null)
                    throw new ArgumentException("Context has already been initialized.");

                _stackAccessor = stackAccessor;
            }

            public void Clear()
            {
                _stackAccessor = null;
            }
        }

        #endregion

        #region Execution

        public static void Execute(Action action)
        {
            Execute(action, null);
        }

        public static void Execute(Action action, object context)
        {
            // Need to specify context explicitly since the Action method is wrapped in a Func delegate.
            ExecuteAndReturn(() => { action(); return true; }, context ?? action.Target);
        }

        public static T ExecuteAndReturn<T>(Func<T> func)
        {
            return ExecuteAndReturn(func, null);
        }

        public static T ExecuteAndReturn<T>(Func<T> action, object context)
        {
            if(_stackAccessor == null)
                throw new NullReferenceException("Context is not initialized.");

            _stackAccessor().Push(context ?? action.Target);

            try
            {
                return action();
            }
            finally
            {
                _stackAccessor().Pop();
            }
        }

        public static T ExecuteAndReturn<T>(object context)
        {
            var method = ContextExecuteMethod(context);
            return (T)ExecuteAndReturn(() => method.Invoke(context, null), context);
        }

        public static void Execute(object context)
        {
            var method = ContextExecuteMethod(context);
            Execute(() => method.Invoke(context, null), context);
        }

        private static MethodInfo ContextExecuteMethod(object context)
        {
            var type = context.GetType();
            var executeMethod = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);

            if (executeMethod == null)
                throw new InvalidOperationException("No Execute method on Context object.");

            return executeMethod;
        }

        #endregion
    }
}
