using System.Collections.Generic;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    public interface Ledgers : ICollection<LedgerEntry>
    {}

    internal static class LedgersTraits
    {
        public static void AddEntry(this Ledgers ledgers, string message, decimal amount)
        {
            ledgers.Add(new LedgerEntry { Message = message, Amount = amount });
        }

        public static decimal Balance(this Ledgers ledgers)
        {
            return ledgers.Sum(e => e.Amount);
        }
    }
}