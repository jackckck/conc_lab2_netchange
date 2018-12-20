using System;
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

        public RoutingTable(int home, int[] neighbours) {
            this.homePort = home;
            this.nodeCount = 1 + neighbours.Length;
            // connections and routes to direct neighbours
            for (int i = 0; i < neighbours.Length; i++) {
                this.neighbourDistances.Add(neighbours[i], new Dictionary<int, int>());
                this.routes.            Add(home, new int[2] { 0, home });
                this.routes.            Add(neighbours[i], new int[2] { 1, neighbours[i] });
            }
        }

        public override string ToString() {
            string res = "";
            foreach (KeyValuePair<int, int[]> route in this.routes) {
                res += route.Key + " " + route.Value[0] + " ";
                // port == eigen port
                if (route.Key == route.Value[1]) res += "local";
                else res += route.Value[1];
                res += "\n";
            }
            return res;
        }

        public void AddConnection(int neighbourPort, Connection c) {
            if (this.neighbourConnections.ContainsKey(neighbourPort)) return;
            this.neighbourConnections.Add(neighbourPort, c);
        }

        // returns null if no connect
        public Connection GetConnection(int neighbourPort) {
            this.neighbourConnections.TryGetValue(neighbourPort, out Connection res);
            return res;
        }

        public void RemoveConnection(int neighbourPort) {
            this.neighbourConnections.Remove(neighbourPort);
            this.neighbourDistances.Remove(neighbourPort);
        }

        public void PurgeNode(int port) {
            this.neighbourConnections.Remove(port);
            this.neighbourDistances.Remove(port);
            this.routes.Remove(port);

            this.nodeCount--;
        }

        // updates the routing table's knowledge of the distance between neighbourPort and destinationPort
        public void UpdateNeighbourDistance(int neighbourPort, int destinationPort, int newDistance) {
            if (this.neighbourDistances.TryGetValue(neighbourPort, out Dictionary<int, int> neighbourDistance)) {
                if (neighbourDistance.TryGetValue(destinationPort, out int steps))
                    steps = newDistance;
                else
                    neighbourDistance.Add(destinationPort, newDistance);
            }
        }

        public void Recompute(int port) {
            int[] newRoute = new int[2];

            // compute new route
            if (this.homePort == port) {
                newRoute[0] = 0; newRoute[1] = this.homePort;
            } else {
                int lowestDistance = this.nodeCount;
                foreach(KeyValuePair<int, Dictionary<int, int>> neighbourDistance in this.neighbourDistances) {
                    // if the neighbour knows its distance to the given node, and its distance is lower than that of all
                    // the other neighbours, it becomes the preferred neighbour
                    if (neighbourDistance.Value.TryGetValue(port, out int stepCount) && stepCount < lowestDistance) {
                        // preferred neighbour's distance to port
                        newRoute[0] = stepCount + 1;
                        // preferred neighbour's port
                        newRoute[1] = neighbourDistance.Key;
                    }
                }
            }

            // update route with new route
            if (this.routes.TryGetValue(port, out int[] route)) {
                route = newRoute;
            } else {
                this.routes.Add(port, newRoute);
                this.nodeCount++;
            }
        }
    }
}