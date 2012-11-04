using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace Ivento.Dci
{
    public class Context
    {
        private static Stack _contextStack;

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
                if (_contextStack == null)
                    throw new ArgumentException("Context has not been initialized.");

                return _contextStack.Peek();
            }
        }

        #region Initializion

        public class ContextInitialization
        {
            private static ThreadLocal<Stack> _threadContextAccessor;

            internal ContextInitialization()
            {}

            public void InThreadScope()
            {
                if (_contextStack != null)
                    throw new ArgumentException("Context has already been initialized.");

                _threadContextAccessor = new ThreadLocal<Stack>(() => new Stack());
                With(_threadContextAccessor.Value);
            }

            public void With(Stack stack)
            {
                if (_contextStack != null)
                    throw new ArgumentException("Context has already been initialized.");

                _contextStack = stack;
            }

            public static void Clear()
            {
                _contextStack = null;
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
            if (_contextStack == null)
                throw new ArgumentException("Context has not been initialized.");

            _contextStack.Push(context ?? action.Target);
            var output = action();
            _contextStack.Pop();

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
