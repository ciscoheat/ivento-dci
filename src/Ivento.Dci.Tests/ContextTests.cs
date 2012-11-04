using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Should.Fluent;

namespace Ivento.Dci.Tests
{
    [TestFixture]
    public class ContextTests
    {
        protected const string StackEmptyMessage = "Stack empty.";

        [SetUp]
        public void BaseSetUp()
        {
            Context.Initialize.Clear();
        }

        public class TheStaticInitializeMethod : ContextTests
        {
            [Test]
            [ExpectedException(typeof(NullReferenceException))]
            public void ShouldClearTheCurrentContextWhenCalledWithClear()
            {
                // Arrange

                // Act
                Context.Initialize.Clear();

                // Assert
                // Accessing an uninitialized context should throw an exception.
                Context.Current.Should().Not.Be.Null();
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfThreadScopeInitializedTwice()
            {
                // Arrange
                Context.Initialize.InThreadScope();

                // Act
                Context.Initialize.InThreadScope();

                // Assert
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfCustomStackInitializedTwice()
            {
                // Arrange
                Context.Initialize.With(() => new Stack());

                // Act
                Context.Initialize.With(() => new Stack());

                // Assert
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = StackEmptyMessage)]
            public void ShouldCreateAStackWhenCalledWithThreadContext()
            {
                // Arrange

                // Act
                Context.Initialize.InThreadScope();

                // Assert
                Context.Current.Should().Not.Be.Null();
            }

            [Test]
            public void ShouldSetTheStackWhenCalledWithTheStackParameterMethod()
            {
                // Arrange
                var stack = new Stack();
                stack.Push("Mock");

                // Act
                Context.Initialize.With(() => stack);

                // Assert
                Context.Current.Should().Equal("Mock");
            }
        }

        public class TheExecuteMethod : ContextTests
        {
            private const string TestPropertyMessage = "Call me maybe";

            [SetUp]
            public void SetUp()
            {
                Context.Initialize.InThreadScope();
            }

            [Test]
            [ExpectedException(typeof(NullReferenceException))]
            public void ShouldThrowExceptionIfNotInitialized()
            {
                // Arrange
                Context.Initialize.Clear();

                // Act
                Context.Execute(() => {});

                // Assert
            }

            [Test]
            public void ShouldPutTheExecutingObjectAsContext()
            {
                // Arrange
                var context = new SimpleContext();

                // Act
                Context.Execute(context.AssignTestPropertyToContext);

                // Assert
                context.TestProperty.Should().Equal(TestPropertyMessage);
            }

            [Test]
            public void CanSetContextSpecificallyByASecondParameter()
            {
                // Arrange
                var context1 = new SimpleContext();
                var context2 = new SimpleContext();

                // Act
                Context.Execute(context1.AssignTestPropertyToContext, context2);

                // Assert
                context2.TestProperty.Should().Equal(TestPropertyMessage);
            }

            [Test]
            public void ShouldBeExecutedAsThreadStaticIfInThreadScope()
            {
                var context = new SimpleContext();

                // Arrange
                Context.Initialize.Clear();

                // Need to emulate the InThreadScope method here for easier testing.
                var stack = new ThreadLocal<Stack>(() => new Stack());
                Context.Initialize.With(() => stack.Value);

                var newThread = new Thread(() => Context.Execute(context.Wait));

                // Act & Assert
                stack.Value.Count.Should().Equal(0);

                // Start the thread, wait for it to set the context.
                newThread.Start();
                Thread.Sleep(10);

                // Other thread is inside its context here, but this context stack should be empty.
                stack.Value.Count.Should().Equal(0);

                newThread.Join(30);
            }

            private static Stack _staticStack;

            [Test]
            public void ShouldBeExecutedAsStaticIfInStaticScope()
            {
                var context = new SimpleContext();

                // Arrange
                Context.Initialize.Clear();

                // Need to emulate the InStaticScope method here for easier testing.
                _staticStack = new Stack();
                Context.Initialize.With(() => _staticStack);

                var newThread = new Thread(() => Context.Execute(context.Wait));

                // Act & Assert
                _staticStack.Count.Should().Equal(0);

                // Start the thread, wait for it to set the context.
                newThread.Start();
                Thread.Sleep(10);

                // Other thread is inside its context here, and since they share context it should be found.
                _staticStack.Count.Should().Equal(1);

                newThread.Join(30);
            }

            [Test]
            public void ShouldReturnAValueIfAFuncIsUsed()
            {
                // Arrange
                var context1 = new SimpleContext();

                // Act
                var output = Context.ExecuteAndReturn(context1.ReturnAValue);

                // Assert
                output.Should().Equal(TestPropertyMessage);
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ShouldThrowExceptionIfAnObjectIsSuppliedThatHasNoExecuteMethod()
            {
                // Arrange
                var context = new SimpleContext();

                // Act
                Context.Execute(context);

                // Assert
            }

            [Test]
            public void ShouldCallTheExecuteMethodIfAnObjectIsSupplied()
            {
                // Arrange
                var context = new ContextWithExecuteMethod();

                // Act
                Context.Execute(context);

                // Assert
                context.ExecuteCalled.Should().Be.True();
            }

            [Test]
            public void ShouldCallTheExecuteMethodAndReturnAValueIfAnObjectIsSuppliedWhereTheExecuteMethodReturnsAValue()
            {
                // Arrange
                var context = new ContextWithExecuteMethodThatReturnsAValue();

                // Act
                var output = Context.ExecuteAndReturn<string>(context);

                // Assert
                output.Should().Equal(TestPropertyMessage);
            }

            [Test]
            public void ShouldSupportNestedContexts()
            {
                // Arrange
                var list = new List<string>();
                var context = new ContextThatTracksStackDepth(list, 3);

                // Act
                Context.Execute(context);

                // Assert
                list.Count.Should().Equal(3);
            }

            #region Mock Contexts

            /// <summary>
            /// Methods in here should not be made static because this instantiated class is used as context.
            /// </summary>
            private class SimpleContext
            {
                internal string TestProperty { get; private set; }

                internal void AssignTestPropertyToContext()
                {
                    var context = Context.CurrentAs<SimpleContext>();
                    context.TestProperty = TestPropertyMessage;
                }

                internal void Wait()
                {
                    Thread.Sleep(20);
                }

                internal void TestIfContextDiffersFrom(SimpleContext otherContext)
                {
                    var context = Context.CurrentAs<SimpleContext>();

                    context.Should().Not.Equal(otherContext);
                }

                internal void TestIfContextIsSameAs(SimpleContext otherContext)
                {
                    var context = Context.CurrentAs<SimpleContext>();

                    context.Should().Equal(otherContext);
                }

                internal string ReturnAValue()
                {
                    return TestPropertyMessage;
                }
            }

            public class ContextWithExecuteMethod
            {
                public bool ExecuteCalled { get; set; }

                public void Execute()
                {
                    ExecuteCalled = true;
                }
            }

            public class ContextWithExecuteMethodThatReturnsAValue
            {
                public string Execute()
                {
                    return TestPropertyMessage;
                }
            }

            public class ContextThatTracksStackDepth
            {
                private readonly List<string> _stackList;
                private readonly int _maxDepth;

                public ContextThatTracksStackDepth(List<string> stackList, int maxDepth)
                {
                    _stackList = stackList;
                    _maxDepth = maxDepth;
                }

                public void Execute()
                {
                    _stackList.Add("Executing Context at depth: " + _stackList.Count);

                    if(_stackList.Count < _maxDepth)
                        Context.Execute(new ContextThatTracksStackDepth(_stackList, _maxDepth));
                }
            }

            #endregion
        }
    }
}
