using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ivento.Dci.Examples.Djikstra.Data
{
    public class ManhattanGeometry
    {
        public const int Infinity = Int32.MaxValue / 4;

        public IList<Node> Nodes { get; set; }
        public Dictionary<Edge, int> Distances { get; set; }

        public Dictionary<Node, Node> EastNeighborOf { get; set; }
        public Dictionary<Node, Node> SouthNeighborOf { get; set; }

        public ManhattanGeometry()
        {
            Nodes = new List<Node>();
            Distances = new Dictionary<Edge, int>();
            EastNeighborOf = new Dictionary<Node, Node>();
            SouthNeighborOf = new Dictionary<Node, Node>();
        }

        /// <summary>
        /// Initialize a map with nodes.
        /// </summary>
        /// <param name="fromATo">End node name. Starting with lowercase a.</param>
        public ManhattanGeometry(char fromATo) : this()
        {
            for (var letter = 'a'; letter <= fromATo; letter++)
            {                
                Nodes.Add(new Node { Name = letter.ToString(CultureInfo.InvariantCulture) });
            }

            foreach (var t in Nodes)
            {
                foreach (var t1 in Nodes)
                {
                    Distances[new Edge(t, t1)] = Infinity;
                }
            }
        }

        protected enum Direction
        {
            East,
            South
        }

        /// <summary>
        /// Convenience method for inherited classes to create paths between nodes.
        /// </summary>
        protected void SetDistance(char a, char b, int distance, Direction dir)
        {
            var node1 = Nodes.Single(n => n.Name == a.ToString(CultureInfo.InvariantCulture));
            var node2 = Nodes.Single(n => n.Name == b.ToString(CultureInfo.InvariantCulture));

            Distances[new Edge(node1, node2)] = distance;

            if (dir == Direction.East)
                EastNeighborOf[node1] = node2;
            else
                SouthNeighborOf[node1] = node2;
        }
    }
}
