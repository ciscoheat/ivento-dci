namespace Ivento.Dci.Examples.Djikstra.Data
{
    public struct Edge
    {
        public readonly Node From;
        public readonly Node To;

        public Edge(Node from, Node to)
        {
            To = to;
            From = from;
        }
    }
}
