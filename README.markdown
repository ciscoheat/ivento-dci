Ivento-DCI is a library for developing C# applications using the DCI programming paradigm.

Right now the documentation is in the examples, so browse to src\Ivento.Dci.Examples.MoneyTransfer\Program.cs for getting started.

For more information about DCI, please see [this very descriptive document](http://www.artima.com/articles/dci_vision.html), [DCI questions on Stackoverflow](http://stackoverflow.com/questions/tagged/dci) or the [object-composition](https://groups.google.com/forum/?fromgroups#!forum/object-composition) group for detailed discussions. For a specific C# implementation, see [this blog post](http://horsdal.blogspot.se/2009/05/dci-in-c.html).

**Please note:** There is a minor rule violation in this library, because of the C# language. There can be naming collisions in extension methods, and when that happens the class method name takes precedence. So class and Role methods having the same name will create problems. This is usually solved with naming conventions for Role methods.
