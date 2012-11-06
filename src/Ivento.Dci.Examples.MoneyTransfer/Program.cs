using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ivento.Dci.Examples.MoneyTransfer.Data;

namespace Ivento.Dci.Examples.MoneyTransfer
{
    class Program
    {
        static void Main()
        {
            //
            // Starting point of MoneyTransfer and PayBills example.
            //
            // This and all context files (MoneyTransfer, PayBills, Account) are commented,
            // so follow the code for an in-code tutorial.
            //

            // The Context must be initialized before use, depending on the type of application. 
            // In a simple application like this, the InStaticScope initalization 
            // can be used. It means that the context will be shared between threads.
            //
            // Multithreading can create unpredictable effects when switching Context, so if the 
            // Context should be scoped per Thread, use InThreadScope.
            // 
            // In a web application, the scope will be per Request and another Initalization
            // method should be called, using for example HttpContext.Items for scope.
            Context.Initialize.InStaticScope();

            // Create some accounts that will be used for the examples.
            // Note that the Account is not a Data entity but a Context. The account balance
            // is calculated using the underlying real Data entity objects, LedgerEntry.
            var source = new Account(new List<LedgerEntry>
                                         {
                                             new LedgerEntry { Message = "Start", Amount = 0m },
                                             new LedgerEntry { Message = "First deposit", Amount = 1000m }
                                         }
                                     );

            var destination = new Account(new List<LedgerEntry>());

            // Output a menu
            Console.WriteLine("DCI Examples");
            Console.WriteLine();
            Console.WriteLine("1 - Basic money transfer");
            Console.WriteLine("2 - Pay bills");
            Console.WriteLine("3 - Pay bills without enough money");
            Console.WriteLine();

            // Detect choice.
            var key = Console.ReadKey(true);

            var menuAction = new Dictionary<ConsoleKey, Action>
                                 {
                                     {ConsoleKey.D1, () => BasicMoneyTransfer(source, destination)},
                                     {ConsoleKey.D2, () => PayBills(ref source)},
                                     {ConsoleKey.D3, () => PayBills(ref source, enoughMoney: false)}
                                 };

            // Run the chosen example. Go to the BasicMoneyTransfer method below for
            // following the code comments.
            if (menuAction.ContainsKey(key.Key))
            {
                menuAction[key.Key]();
            }
            else
            {
                Console.WriteLine("No example selected.");
            }

            // Output the account ledgers after the examples are done.

            if (source.LedgersList.Any())
            {
                Console.WriteLine();
                Console.WriteLine("SOURCE LEDGERS");
                source.LedgersList.ToList().ForEach(Console.WriteLine);
            }

            if (destination.LedgersList.Any())
            {
                Console.WriteLine();
                Console.WriteLine("DESTINATION LEDGERS");
                destination.LedgersList.ToList().ForEach(Console.WriteLine);
            }

            Console.WriteLine();
            Console.WriteLine("Press the any key to exit.");
            Console.ReadKey(true);

            // Example finished!
        }

        /// <summary>
        /// First example. Please open the MoneyTransfer.cs file for an in-depth
        /// explanation of a Context.
        /// </summary>
        private static void BasicMoneyTransfer(Account source, Account destination)
        {
            // Create the context using the supplied accounts. 245 will be the transfer amount.
            var context = new MoneyTransfer(source, destination, 245m);

            Console.WriteLine("BEFORE");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
            Console.WriteLine("Destination account      " + destination.Balance.ToString("c"));

            Console.WriteLine();
            Console.WriteLine("Transfering from Source to Destination: " + context.Amount.ToString("c"));

            // Execute the context.
            context.Execute();

            Console.WriteLine();
            Console.WriteLine("AFTER");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
            Console.WriteLine("Destination account      " + destination.Balance.ToString("c"));
        }

        private static void PayBills(ref Account source, bool enoughMoney = true)
        {
            // Create some creditors that wants money from the supplied account.
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

            // Create a new account with little money if enoughMoney is false.
            if (!enoughMoney)
            {
                source = new Account(new List<LedgerEntry>
                                         {
                                             new LedgerEntry {Message = "From mom", Amount = 80m}
                                         }
                                    );
            }


            // Create the context using the supplied account and the creditors.
            var context = new PayBills(source, creditors);

            Console.WriteLine("BEFORE");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));

            Console.WriteLine();
            Console.WriteLine("PAYING BILLS");
            creditors.ForEach(Console.WriteLine);

            // Execute the context.
            try
            {
                context.Execute();
            }
            catch (AccountException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("AFTER");
            Console.WriteLine("Source account           " + source.Balance.ToString("c"));
        }
    }
}
