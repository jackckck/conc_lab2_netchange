using System.Linq;
using System.Collections.Generic;

namespace lab2 {
    public class RoutingTable {
        private int homePort;
        private int nodeCount;

        // key ^= neighbour's port int, value ^= Connection to neighbour
        private Dictionary<int, Connection> neighbourConnections = new Dictionary<int, Connection>();
        // key ^= neighbour's port int, value ^= dictionary for which: key ^= node, value ^= neighbour's distance to node
        private Dictionary<int, Dictionary<int, int>> neighbourDistances = new Dictionary<int, Dictionary<int, int>>();
        // if route = routes[key], then route[0] is the amount of steps, and route[1] the preferred neighbour
        private Dictionary<int, int[]> routes = new Dictionary<int, int[]>();

        public RoutingTable(int homePort) {
            this.homePort = homePort;
            this.routes[homePort] = new int[2] { 0, homePort };
            //this.nodeCount = 1 + neighbours.Length;
        }

        public override string ToString() {
            string res = "";
            foreach (KeyValuePair<int, int[]> route in this.routes) {
                res += route.Key + " " + route.Value[0] + " ";
                if (route.Key == this.homePort) res += "local";
                else res += route.Value[1];
                res += "\n";
            }

            return res;
        }

        // returns null if no route
        public Connection GetConnection(int farPort) {
            if (this.routes.TryGetValue(farPort, out int[] route) &&
                this.neighbourConnections.TryGetValue(route[1], out Connection res))
                    return res;
            return null;
        }

        // get all the neighbour connections
        public Connection[] GetNeighbourConnections() {
            return neighbourConnections.Values.ToArray();
        }


        // add a neighbour
        public void AddNeighbour(int neighbourPort, Connection c) {
            this.neighbourConnections[neighbourPort] = c;
            this.neighbourDistances[neighbourPort] = new Dictionary<int, int>();

            // new neighbour is its own preferred neighbour
            this.UpdateRoute(neighbourPort, 1, neighbourPort);
        }

        // remove a connection
        public bool RemoveNeighbour(int neighbourPort) {
            this.neighbourConnections.Remove(neighbourPort);
            this.neighbourDistances.Remove(neighbourPort);

            return Recompute(neighbourPort);
        }

        // updates the routing table's knowledge of the distance between neighbourPort and destinationPort
        public bool UpdateNeighbourDistance(int neighbourPort, int farPort, int newDistance) {
            if (this.neighbourDistances.TryGetValue(neighbourPort, out Dictionary<int, int> distances)) {
                distances[farPort] = newDistance;
            }
            return Recompute(farPort);
        }

        // use with care, or just use recompute instead
        // todo check for bugs
        private void UpdateRoute(int port, int newDistance, int newPreferred) {
            this.routes[port] = new int[2] { newDistance, newPreferred };
        }
        
        private void PurgeNode(int port) {
            this.neighbourConnections.Remove(port);
            this.neighbourDistances.Remove(port);
            this.routes.Remove(port);

            //this.nodeCount--;
        }

        private bool Recompute(int farPort) {
            int[] newRoute = new int[2];

            if (this.homePort == farPort) {
                this.routes[farPort] = new int[2] { 0, homePort };
                return false;
            }

            // compute new route

            // int lowestDistance = this.nodeCount;
            int lowestDistance = 20;
            foreach (KeyValuePair<int, Dictionary<int, int>> neighbourDistance in this.neighbourDistances) {
                // if the neighbour knows its distance to the given node, and its distance is lower than that of all
                // the other neighbours, it becomes the preferred neighbour
                if (neighbourDistance.Value.TryGetValue(farPort, out int stepCount) && stepCount < lowestDistance) {
                    lowestDistance = stepCount;
                    // preferred neighbour's distance to port + 1
                    newRoute[0] = stepCount + 1;
                    // preferred neighbour's port
                    newRoute[1] = neighbourDistance.Key;
                }
            }

            // update route with new route
            bool res = this.routes.TryGetValue(farPort, out int[] route) && route == newRoute;
            this.routes[farPort] = newRoute;
            return res;
        }
    }
}