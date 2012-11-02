using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    class MoneyTransfer
    {
        public SourceAccount Source { get; private set; }
        public DestinationAccount Destination { get; private set; }
        public decimal Amount { get; private set; }

        public MoneyTransfer(SourceAccount source, DestinationAccount destination, decimal amount)
        {
            Source = source;
            Destination = destination;
            Amount = amount;
        }

        public void Execute()
        {
            Source.Transfer();
        }
    }

    public interface SourceAccount
    {
        void Withdraw(decimal amount);
    }

    public interface DestinationAccount
    {
        void Deposit(decimal amount);
    }

    public static class SourceAccountTraits
    {
        public static void Transfer(this SourceAccount source)
        {
            var context = Context.CurrentAs<MoneyTransfer>();

            context.Destination.Deposit(context.Amount);
            source.Withdraw(context.Amount);
        }
    }
}
