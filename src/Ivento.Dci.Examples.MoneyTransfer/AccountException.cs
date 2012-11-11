using System;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    /// <summary>
    /// Thrown when an Account operation fails.
    /// </summary>
    public class AccountException : Exception
    {
        public AccountException(string message) : base(message)
        {}
    }
}