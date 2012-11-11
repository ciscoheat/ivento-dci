using System;
using System.Collections.Generic;
using Ivento.Dci.Examples.Djikstra.Data;
using System.Linq;

namespace Ivento.Dci.Examples.Djikstra.Contexts
{
    public sealed class CalculateShortestPath
    {
        internal const int Infinity = int.MaxValue / 4;

        internal Node Origin { get; private set; }
        internal Node Destination { get; private set; }

        internal Node Current { get; private set; }

        internal Node EastNeighbor { get { return Map.EastNeighborOf.ContainsKey(Current) ? Map.EastNeighborOf[Current] : null; } }
        internal Node SouthNeighbor { get { return Map.SouthNeighborOf.ContainsKey(Current) ? Map.SouthNeighborOf[Current] : null; } }

        internal IDictionary<Node, int> TentativeDistance;
        internal IDictionary<Node, Node> PathTo { get; private set; }
        internal HashSet<Node> Unvisited { get; private set; }

        #region Roles and Role Contracts

        public interface CurrentIntersectionRole
        {}

        public interface DistanceLabeledGraphNodeRole
        {}

        public interface NeighborNodeRole
        {}

        internal MapRole Map { get; private set; }
        public interface MapRole
        {
            Dictionary<Edge, int> Distances { get; set; }

            Dictionary<Node, Node> EastNeighborOf { get; set; }
            Dictionary<Node, Node> SouthNeighborOf { get; set; }
        }

        #endregion

        #region RolePlayers

        #endregion

        #region Constructors and Role bindings

        public CalculateShortestPath(Node origin, Node target, ManhattanGeometry geometry)
        {
            BindRoles(origin, target, geometry);
        }

        private void BindRoles(Node origin, Node target, ManhattanGeometry geometry)
        {
            // Variable initialization
            PathTo = new Dictionary<Node, Node>();
            Origin = origin;
            Destination = target;

            // Assign to every node a tentative distance value: 
            // set it to zero for our initial node and to infinity for all other nodes.
            TentativeDistance = new Dictionary<Node, int>(geometry.Nodes.Count);
            geometry.Nodes.ToList().ForEach(node => TentativeDistance[node] = Infinity);
            TentativeDistance[origin] = 0;

            // Mark all nodes unvisited. 
            // Set the initial node as current. 
            // Create a set of the unvisited nodes called the unvisited set 
            // consisting of all the nodes except the initial node.
            Unvisited = new HashSet<Node>(geometry.Nodes);
            Unvisited.Remove(origin);
            Current = origin;

            // Bind RolePlayers to Roles
            Map = geometry.ActLike<MapRole>();
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

            var unvisitedNeighbors = Current.ActLike<CurrentIntersectionRole>().UnvisitedNeighbors();

            if(unvisitedNeighbors.Count > 0)
            {
                foreach (var neighbor in unvisitedNeighbors.AllActLike<NeighborNodeRole>())
                {
                    var netDistance = Current.ActLike<DistanceLabeledGraphNodeRole>().TentativeDistance() + 
                        Map.DistanceBetween(Current, neighbor.IsA<Node>());

                    if (neighbor.RelableNodeAs(netDistance))
                        PathTo[neighbor.IsA<Node>()] = Current;
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
                // and go back to step 3.
                var nextContext = CloneContext(Map.NearestUnvisitedNodeToTarget());
                return Context.ExecuteAndReturn<List<Node>>(nextContext);
            }
            
            return GeneratePath();
        }

        #endregion

        private List<Node> GeneratePath()
        {
            var output = new List<Node>();
            var n = Destination;

            while (n != Origin)
            {
                output.Insert(0, n);
                n = PathTo[n];
            }

            output.Insert(0, Origin);
            return output;
        }

        private CalculateShortestPath CloneContext(Node nextNode)
        {
            var clone = (CalculateShortestPath) MemberwiseClone();
            clone.Current = nextNode;

            return clone;
        }
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

            var min = CalculateShortestPath.Infinity;
            Node selected = null;

            foreach (var node in context.Unvisited.AllActLike<CalculateShortestPath.DistanceLabeledGraphNodeRole>())
            {
                var distance = node.TentativeDistance();
                if (distance >= min) continue;

                selected = node.IsA<Node>();
                min = distance;
            }

            return selected;
        }

        #endregion

        #region CurrentIntersection

        public static IList<Node> UnvisitedNeighbors(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var output = new List<Node>();

            var unvisited = currentIntersection.Unvisited();
            var south = currentIntersection.SouthNeighbor();
            var east = currentIntersection.EastNeighbor();

            if (south != null && unvisited.Contains(south))
                output.Add(south);

            if (east != null && unvisited.Contains(east))
                output.Add(east);

            return output;
        }

        public static HashSet<Node> Unvisited(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.Current);
            return context.Unvisited;
        }

        public static Node SouthNeighbor(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.Current);
            return context.SouthNeighbor;
        }

        public static Node EastNeighbor(this CalculateShortestPath.CurrentIntersectionRole currentIntersection)
        {
            var context = Context.Current<CalculateShortestPath>(currentIntersection, c => c.Current);
            return context.EastNeighbor;
        }

        #endregion

        #region DistanceLabeledGraphNode

        public static int TentativeDistance(this CalculateShortestPath.DistanceLabeledGraphNodeRole node)
        {
            var context = Context.Current<CalculateShortestPath>();

            return context.TentativeDistance[node.IsA<Node>()];
        }

        public static void SetTentativeDistance(this CalculateShortestPath.DistanceLabeledGraphNodeRole node, int distance)
        {
            var context = Context.Current<CalculateShortestPath>();

            context.TentativeDistance[node.IsA<Node>()] = distance;
        }

        #endregion

        #region Neighbor

        public static bool RelableNodeAs(this CalculateShortestPath.NeighborNodeRole node, int distance)
        {
            var distanceNode = node.ActLike<CalculateShortestPath.DistanceLabeledGraphNodeRole>();

            if(distance < distanceNode.TentativeDistance())
            {
                distanceNode.SetTentativeDistance(distance);
                return true;
            }

            return false;
        }

        #endregion
    }

    #endregion
}
