using System.Collections.Generic;
using Ivento.Dci.Examples.Djikstra.Data;
using System.Linq;

namespace Ivento.Dci.Examples.Djikstra.Contexts
{
    /// <summary>
    /// DCI Implementation of the Djikstra algorithm.
    /// Based on code from http://fulloo.info/Examples/RubyExamples/Dijkstra/
    /// Comments from Wikipedia: http://en.wikipedia.org/wiki/Dijkstra's_algorithm
    /// Visual explanation of the algorithm: http://www.youtube.com/watch?v=psg2-6-CEXg
    /// </summary>
    /// <remarks>
    /// The MoneyTransfer example is a much more detailed explanation of DCI.
    /// Use that for a beginners tutorial.
    /// </remarks>
    public sealed class CalculateShortestPath
    {
        #region Roles and Role Contracts

        internal IDictionary<Node, int> TentativeDistance { get; private set; }

        internal UnvisitedRole Unvisited { get; private set; }
        public interface UnvisitedRole : IEnumerable<Node>
        {
            void Remove(Node node);
        }

        // The Current node plays two different roles: CurrentIntersection and DistanceGraph.
        // They only have methodful roles so they are implemented as an empty interface.
        internal Node Current { get; private set; }
        public interface CurrentRole {}

        internal CurrentIntersectionRole CurrentIntersection { get { return Current; } }
        public interface CurrentIntersectionRole {}

        internal DistanceGraphRole DistanceGraph { get { return Current; } }
        public interface DistanceGraphRole {}

        // The Neighbor Role is played by nodes to the Current role.
        public interface NeighborRole {}

        // The Map role implements from the ManhattanGeometry entity.
        internal MapRole Map { get; private set; }
        public interface MapRole
        {
            Dictionary<Edge, int> Distances { get; set; }

            Dictionary<Node, Node> EastNeighborOf { get; set; }
            Dictionary<Node, Node> SouthNeighborOf { get; set; }
        }

        #endregion

        #region Private variables

        // Storage for origin, destination and best path.
        private Node _origin;
        private Node _destination;
        private IDictionary<Node, Node> _pathTo;

        private class UnvisitedNodes : HashSet<Node>, UnvisitedRole
        {
            public UnvisitedNodes(IEnumerable<Node> nodes) : base(nodes)
            {}

            public new void Remove(Node node)
            {
                base.Remove(node);
            }
        }

        #endregion

        #region Constructors and Role bindings

        public CalculateShortestPath(Node origin, Node target, ManhattanGeometry geometry)
        {
            BindRoles(origin, target, geometry);
        }

        private void BindRoles(Node origin, Node target, ManhattanGeometry geometry)
        {
            // Variable initialization
            _pathTo = new Dictionary<Node, Node>();
            _origin = origin;
            _destination = target;

            // Bind RolePlayers to Roles
            Current = origin; // Set the initial node as current. 
            Map = geometry;

            // A set of the unvisited nodes called the unvisited set consisting of all the nodes 
            // except the initial node.
            Unvisited = new UnvisitedNodes(geometry.Nodes);
            Unvisited.Remove(origin);

            // Assign to every node a tentative distance value: 
            // Set it to zero for our initial node and to infinity for all other nodes.
            TentativeDistance = geometry.Nodes.ToDictionary(n => n, n => ManhattanGeometry.Infinity);
            TentativeDistance[origin] = 0;
        }

        #endregion

        #region Context members

        public List<Node> Execute()
        {
            return Context.ExecuteAndReturn(CalculatePath);
        }

        private List<Node> CalculatePath()
        {
            // For the current node, consider all of its unvisited neighbors and calculate their tentative distances. 
            // For example, if the current node A is marked with a tentative distance of 6, and the edge connecting 
            // it with a neighbor B has length 2, then the distance to B (through A) will be 6+2=8. If this distance 
            // is less than the previously recorded tentative distance of B, then overwrite that distance. 
            // Even though a neighbor has been examined, it is not marked as "visited" at this time, and it 
            // remains in the unvisited set.

            var unvisitedNeighbors = CurrentIntersection.UnvisitedNeighbors();

            if(unvisitedNeighbors.Count > 0)
            {
                foreach (var neighbor in unvisitedNeighbors)
                {                    
                    var netDistance = DistanceGraph.TentativeDistance() + Map.DistanceBetween(Current, neighbor);

                    if (neighbor.RelableNodeAs(netDistance))
                    {
                        _pathTo[neighbor] = Current;
                    }
                }
            }

            // When we are done considering all of the neighbors of the current node, mark the current node 
            // as visited and remove it from the unvisited set. A visited node will never be checked again; 
            // its distance recorded now is final and minimal.
            Unvisited.Remove(Current);

            // If the destination node has been marked visited (when planning a route between two specific nodes) 
            // or if the smallest tentative distance among the nodes in the unvisited set is infinity (when 
            // planning a complete traversal), then stop. The algorithm has finished.
            if (Unvisited.Any())
            {
                // Set the unvisited node marked with the smallest tentative distance as the next "current node" 
                // and calculate next unvisited node.
                var nextContext = CloneContext(Map.NearestUnvisitedNodeToTarget());
                return Context.ExecuteAndReturn<List<Node>>(nextContext);
            }
            
            return GeneratePath();
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Call at the end of CalculatePath. Generate the best node path.
        /// </summary>
        private List<Node> GeneratePath()
        {
            var output = new List<Node>();
            var n = _destination;

            while (n != _origin)
            {
                output.Insert(0, n);
                n = _pathTo[n];
            }

            output.Insert(0, _origin);
            return output;
        }

        private CalculateShortestPath CloneContext(Node nextNode)
        {
            // The only thing that need to change is the Current node,
            // so the easiest way is to clone the Context and change context.Current.
            var clone = (CalculateShortestPath)MemberwiseClone();
            clone.Current = nextNode;

            return clone;
        }

        #endregion
    }

    #region Methodful Roles

    static class CalculateShortestPathMethodfulRoles
    {
        #region Map

        public static int DistanceBetween(this CalculateShortestPath.MapRole map, Node a, Node b)
        {
            return map.Distances[new Edge(a, b)];
        }

        public static Node NextDownTheStreetFrom(this CalculateShortestPath.MapRole map, Node x)
        {
            return map.EastNeighborOf[x];
        }

        public static Node NextAlongTheAvenueFrom(this CalculateShortestPath.MapRole map, Node x)
        {
            return map.SouthNeighborOf[x];
        }

        public static Node NearestUnvisitedNodeToTarget(this CalculateShortestPath.MapRole map)
        {
            var context = Context.Current<CalculateShortestPath>(map, c => c.Map);

            var min = ManhattanGeometry.Infinity;
            Node selected = null;
            
            foreach (var node in context.Unvisited)
            {
                var distance = node.TentativeDistance();
                if (distance >= min) continue;

                selected = node;
                min = distance;
            }

            return selected;
        }

        #endregion

        #region CurrentIntersection

        public static IList<Node> UnvisitedNeighbors(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.CurrentIntersection);

            var output = new List<Node>();

            var unvisited = context.Unvisited;
            var south = currentIntersection.SouthNeighbor();
            var east = currentIntersection.EastNeighbor();

            if (south != null && unvisited.Contains(south))
                output.Add(south);

            if (east != null && unvisited.Contains(east))
                output.Add(east);

            return output;
        }

        public static Node SouthNeighbor(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.CurrentIntersection);

            var neighborOf = context.Map.SouthNeighborOf;
            return neighborOf.ContainsKey(context.Current) ? neighborOf[context.Current] : null;
        }

        public static Node EastNeighbor(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.CurrentIntersection);

            var neighborOf = context.Map.EastNeighborOf;
            return neighborOf.ContainsKey(context.Current) ? neighborOf[context.Current] : null;
        }

        #endregion

        #region DistanceGraph

        public static int TentativeDistance(this CalculateShortestPath.DistanceGraphRole distanceGraph)
        {
            var context = Context.Current<CalculateShortestPath>();

            return context.TentativeDistance[(Node)distanceGraph];
        }

        public static void SetTentativeDistance(this CalculateShortestPath.DistanceGraphRole distanceGraph, int distance)
        {
            var context = Context.Current<CalculateShortestPath>();

            context.TentativeDistance[(Node)distanceGraph] = distance;
        }

        #endregion

        #region Neighbor

        public static bool RelableNodeAs(this CalculateShortestPath.NeighborRole neighbor, int distance)
        {
            var distanceGraph = (CalculateShortestPath.DistanceGraphRole)neighbor;

            if(distance < distanceGraph.TentativeDistance())
            {
                distanceGraph.SetTentativeDistance(distance);
                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion
}
