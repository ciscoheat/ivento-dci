using Ivento.Dci.Examples.Djikstra.Data;

namespace Ivento.Dci.Examples.Djikstra
{
    class LittleCityMap : ManhattanGeometry
    {
        public LittleCityMap() : base('i')
        {
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
    }
}
