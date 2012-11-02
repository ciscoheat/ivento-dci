using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    class PayBills
    {
        public SourceAccount Source { get; private set; }
        public IEnumerable<Creditor> Creditors { get; private set; }

        public PayBills(SourceAccount source, IEnumerable<Creditor> creditors)
        {
            Source = source;
            Creditors = creditors;
        }

        public void Execute()
        {
            Source.PayBills();
        }
    }
}
