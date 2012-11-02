using System;
using System.Collections;
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
            Context.Clear();
        }

        public class TheStaticInitializeMethod : ContextTests
        {
            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldClearTheCurrentContextWhenCalledWithClearContextEnum()
            {
                // Arrange

                // Act
                Context.Clear();

                // Assert
                // Accessing an uninitialized context should throw an exception.
                Context.Current.Should().Not.Be.Null();
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfThreadScopeInitializedTwice()
            {
                // Arrange
                Context.InitializeWithThreadScope();

                // Act
                Context.InitializeWithThreadScope();

                // Assert
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfCustomStackInitializedTwice()
            {
                // Arrange
                Context.Initialize(new Stack());

                // Act
                Context.Initialize(new Stack());

                // Assert
            }

            [Test]
            [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = StackEmptyMessage)]
            public void ShouldCreateAStackWhenCalledWithThreadContext()
            {
                // Arrange

                // Act
                Context.InitializeWithThreadScope();

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
                Context.Initialize(stack);

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
                Context.InitializeWithThreadScope();
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfNotInitialized()
            {
                // Arrange
                Context.Clear();

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
            public void ShouldBeExecutedAsThreadStatic()
            {
                var context1 = new SimpleContext();
                var context2 = new SimpleContext();

                // Arrange                
                var newThread = new Thread(() => Context.Execute(context2.Wait));

                // Act

                // Start the thread, wait for it to set the context.
                newThread.Start();
                Thread.Sleep(10);

                // Test if current context is different from the one in the other thread.
                Context.Execute(() => context1.TestIfContextDiffersFrom(context2), context1);
                newThread.Join(30);

                // Assert
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

                internal string ReturnAValue()
                {
                    return TestPropertyMessage;
                }
            }
        }
    }
}
