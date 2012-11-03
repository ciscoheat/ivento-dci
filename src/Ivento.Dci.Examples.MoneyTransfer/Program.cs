using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ImpromptuInterface;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    class Program
    {
        static void Main()
        {
            // The Context must be initialized before use, depending on the type of
            // application. In a simple non-web application like this, the ThreadScope
            // initalization can be used. It means that the context will be scoped per Thread.
            // 
            // In a web application, the scope will be per Request and another Initalization
            // method should be called.
            Context.InitializeWithThreadScope();

            // Create some accounts
            var source = new Account(new List<LedgerEntry>
                                         {
                                             new LedgerEntry { Message = "Start", Amount = 0m },
                                             new LedgerEntry { Message = "First deposit", Amount = 1000m }
                                         }
                                     );

            var destination = new Account(new List<LedgerEntry>());

            // Menu
            Console.WriteLine("1 - Basic money transfer");
            Console.WriteLine("2 - Pay bills");
            Console.WriteLine();

            // Detecting a key can be so ugly...
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
            } while (key.Key != ConsoleKey.D1 && key.Key != ConsoleKey.D2);            

            switch (key.Key)
            {
                case ConsoleKey.D1:
                    BasicMoneyTransfer(destination, source);
                    break;

                case ConsoleKey.D2:
                    PayBills(source);
                    break;
            }

            // Final output

            if (source.Ledgers.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("SOURCE LEDGERS");
                source.Ledgers.ToList().ForEach(Console.WriteLine);
            }

            if (destination.Ledgers.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("DESTINATION LEDGERS");
                destination.Ledgers.ToList().ForEach(Console.WriteLine);
            }

            Console.WriteLine();
            Console.WriteLine("Press the any key to exit.");
            Console.ReadKey(true);
        }

        private static void PayBills(Account source)
        {
            var creditors = new List<Creditor>
                                {
                                    new Creditor
                                        {
                                            Name = "Baker",
                                            Account = new Account(new Collection<LedgerEntry>()),
                                            AmountOwed = 150m
                                        },

                                    new Creditor
                                        {
                                            Name = "Butcher",
                                            Account = new Account(new Collection<LedgerEntry>()),
                                            AmountOwed = 200m
                                        }
                                };

            var context = new PayBills(source.ActLike<SourceAccount>(), creditors);

            Console.WriteLine("BEFORE");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));

            Console.WriteLine();
            Console.WriteLine("PAYING BILLS");
            creditors.ForEach(Console.WriteLine);

            Context.Execute(context);

            Console.WriteLine();
            Console.WriteLine("AFTER");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
        }

        private static void BasicMoneyTransfer(Account destination, Account source)
        {
            // Create the context. Note that the Account objects are duck typed to act 
            // like the RolePlayers they should be in the Context.
            var context = new MoneyTransfer(
                source.ActLike<SourceAccount>(),
                destination.ActLike<DestinationAccount>(),
                245m);

            Console.WriteLine("BEFORE");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
            Console.WriteLine("Destination account      " + destination.Balance.ToString("c"));

            Console.WriteLine();
            Console.WriteLine("Transfering from Source to Destination: " + context.Amount.ToString("c"));

            // Execute the context method.
            Context.Execute(context);

            Console.WriteLine();
            Console.WriteLine("AFTER");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
            Console.WriteLine("Destination account      " + destination.Balance.ToString("c"));
        }
    }
}
