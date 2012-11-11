using System.Collections.Generic;

namespace Ivento.Dci.Examples.Djikstra.Data
{
    public class ManhattanGeometry
    {
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
    }
}
