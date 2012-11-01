using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                var c = Context.Current;
            }

            [Test]
            [ExpectedException(typeof(ArgumentException))]
            public void ShouldThrowExceptionIfInitializedTwice()
            {
                // Arrange
                Context.InitializeWithThreadScope();

                // Act
                Context.InitializeWithThreadScope();

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
                var c = Context.Current;
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
            public string TestProperty { get; set; }

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
                TestProperty = null;

                // Act
                Context.Execute(ExecuteMe);

                // Assert
                TestProperty.Should().Equal("Call me maybe");
            }

            [Test]
            public void ShouldBeExecutedInAThreadStaticContext()
            {
                // Arrange                

                // Act
                //Context.Execute(ExecuteMe);

                // Assert
                var newThread = new Thread(
                    () => Assert.Throws(typeof(InvalidOperationException), 
                        () => { var c = Context.Current; }, StackEmptyMessage));

                newThread.Start();
                newThread.Join(1000);
            }
            
            private void ExecuteMe()
            {
                var context = Context.As<TheExecuteMethod>();

                context.TestProperty = "Call me maybe";
            }
        }
    }
}
