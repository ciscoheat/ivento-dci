using System;
using System.Linq;
using Ivento.Dci.Examples.Djikstra.Contexts;

namespace Ivento.Dci.Examples.Djikstra
{
    /// <summary>
    /// Example of using DCI for the Djikstra pathfinding algorithm.
    /// http://en.wikipedia.org/wiki/Dijkstra's_algorithm
    /// </summary>
    /// <remarks>
    /// The MoneyTransfer example is a much more detailed explanation of DCI.
    /// Use that for a beginners tutorial.
    /// </remarks>
    class Program
    {
        static void Main()
        {
            // Initialize the context as a static (global) scope.
            Context.Initialize.InStaticScope();

            // Create and print the city map to use.
            var map = new CityMap();

            Console.WriteLine("City map:");
            Console.WriteLine();
            Console.WriteLine(map);

            // Pick origin and destination nodes for path.
            var origin = map.Nodes.Single(n => n.Name == "a");
            var destination = map.Nodes.Single(n => n.Name == "i");

            // Execute the Context.
            var path = new CalculateShortestPath(origin, destination, map).Execute();

            // Print the result.
            Console.WriteLine();
            Console.WriteLine(string.Format("Best path from {0} to {1}:", origin, destination));
            Console.WriteLine(string.Join(" -> ", path));

            Console.ReadKey(true);
        }
    }
}
