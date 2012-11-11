using System.Collections.Generic;
using System.Globalization;
using Ivento.Dci.Examples.Djikstra.Contexts;
using Ivento.Dci.Examples.Djikstra.Data;

namespace Ivento.Dci.Examples.Djikstra
{
    class CityMap : ManhattanGeometry
    {
        private Dictionary<char, Node> _node;

        public override string ToString()
        {
            return @"
a - 2 - b - 3 - c
|       |       |
1       2       1
|       |       |
d - 1 - e - 1 - f
|               |
2               4
|               |
g - 1 - h - 2 - i
".Trim();
        }

        private enum Direction
        {
            East,
            South
        }

        public CityMap()
        {
            CreateNodesAndDistances("abcdefghi");

            SetDistance('a', 'b', 2, Direction.East);
            SetDistance('b', 'c', 3, Direction.East);
            SetDistance('c', 'f', 1, Direction.South);
            SetDistance('f', 'i', 4, Direction.South);
            SetDistance('b', 'e', 2, Direction.South);
            SetDistance('d', 'e', 1, Direction.East);
            SetDistance('e', 'f', 1, Direction.East);
            SetDistance('a', 'd', 1, Direction.South);
            SetDistance('d', 'g', 2, Direction.South);
            SetDistance('g', 'h', 1, Direction.East);
            SetDistance('h', 'i', 2, Direction.East);
        }

        private void CreateNodesAndDistances(string nodes)
        {
            const int infinity = CalculateShortestPath.Infinity;

            _node = new Dictionary<char, Node>();

            foreach (var letter in nodes.ToCharArray())
            {
                _node[letter] = new Node {Name = letter.ToString(CultureInfo.InvariantCulture)};
                Nodes.Add(_node[letter]);
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                for (var j = 0; j < nodes.Length; j++)
                {
                    Distances[new Edge(Nodes[i], Nodes[j])] = infinity;
                }
            }
        }

        private void SetDistance(char a, char b, int distance, Direction dir)
        {
            Distances[new Edge(_node[a], _node[b])] = distance;

            if (dir == Direction.East)
                EastNeighborOf[_node[a]] = _node[b];
            else
                SouthNeighborOf[_node[a]] = _node[b];
        }
    }
}
