using System;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public class AccountException : Exception
    {
        public AccountException(string message) : base(message)
        {}
    }
}