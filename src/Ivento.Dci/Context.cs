using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace Ivento.Dci
{
    public class Context
    {
        private static Func<Stack> _stackAccessor;

        private static readonly Lazy<ContextInitialization> InitializeLazy = new Lazy<ContextInitialization>(() => new ContextInitialization());
        public static ContextInitialization Initialize { get { return InitializeLazy.Value; } }

        public static T CurrentAs<T>() where T : class
        {
            return Current as T;
        }

        public static object Current
        {
            get
            {
                return _stackAccessor().Peek();
            }
        }

        #region Initialization

        public class ContextInitialization
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
            var output = action();
            _stackAccessor().Pop();

            return output;
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
