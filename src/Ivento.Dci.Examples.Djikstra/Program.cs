using System;
using System.Linq;
using Ivento.Dci.Examples.Djikstra.Contexts;

namespace Ivento.Dci.Examples.Djikstra
{
    /// <summary>
    /// DCI Implementation of the Djikstra algorithm.
    /// Based on code from http://fulloo.info/Examples/RubyExamples/Dijkstra/
    /// Visual explanation of the algorithm: http://www.youtube.com/watch?v=psg2-6-CEXg
    /// </summary>
    /// <remarks>
    /// The MoneyTransfer example is a much more detailed explanation of DCI.
    /// Use that for a beginners tutorial.
    /// </remarks>
    class Program
    {
        static void Main()
        {
            Context.Initialize.InStaticScope();

            var map = new CityMap();

            Console.WriteLine("City map:");
            Console.WriteLine();
            Console.WriteLine(map);            

            var path = new CalculateShortestPath(map.Nodes[0], map.Nodes[8], map).Execute();

            Console.WriteLine();
            Console.WriteLine("Best path from a to i:");
            Console.WriteLine(string.Join(" -> ", path.Select(n => n.ToString())));

            Console.ReadKey(true);
        }
    }
}
