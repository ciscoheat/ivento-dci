﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImpromptuInterface;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            Context.InitializeWithThreadScope();

            var source = new Account(new List<LedgerEntry>
                                         {
                                             new LedgerEntry("Start", 0m),
                                             new LedgerEntry("First deposit", 1000m)
                                         }
                                     );

            var destination = new Account(new List<LedgerEntry>());

            var context = new MoneyTransfer(
                source.ActLike<SourceAccount>(),
                destination.ActLike<DestinationAccount>(),
                245m);

            Console.WriteLine("BEFORE");
            Console.WriteLine("Source account: " + source.Balance);
            Console.WriteLine("Dest account: " + destination.Balance);

            Context.Execute(context.Execute);

            Console.WriteLine();
            Console.WriteLine("AFTER");
            Console.WriteLine("Source account: " + source.Balance);
            Console.WriteLine("Dest account: " + destination.Balance);

            Console.ReadKey(true);
        }
    }
}
