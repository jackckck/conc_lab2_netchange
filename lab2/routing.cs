using System;
using System.Collections.Generic;

namespace lab2 {
    public class RoutingTable {
        private int homePort;
        private int nodeCount;

        // key ^= neighbour's port int, value ^= Connection to neighbour
        private Dictionary<int, Connection> neighbourConnections = new Dictionary<int, Connection>();
        // key ^= neighbour's port int, value ^= dictionary for which: key ^= node, value ^= neighbour's distance to node
        private Dictionary<int, Dictionary<int, int>> neighbourSteps = new Dictionary<int, Dictionary<int, int>>();
        // if route = routes[key], then route[0] is the amount of steps, and route[1] the preferred neighbour
        private Dictionary<int, int[]> routes = new Dictionary<int, int[]>();

        public RoutingTable(int home, int[] neighbours) {
            this.homePort = home;
            this.nodeCount = 1 + neighbours.Length;
            // connections and routes to direct neighbours
            for (int i = 0; i < neighbours.Length; i++) {
                this.neighbourSteps.Add(neighbours[i], new Dictionary<int, int>());
                this.routes.        Add(home, new int[2] { 0, home });
                this.routes.        Add(neighbours[i], new int[2] { 1, neighbours[i] });
            }
        }

        public int[] TryGetRoute(int port) {
            // no route ;(
            int[] res = new int[2] { -1, -1 };
            this.routes.TryGetValue(port, out res);
            return res;
        }

        public void UpdateNeighbourSteps(int neighbour, int port, int steps) {
            if (!this.neighbourSteps.ContainsKey(neighbour)) {
                Console.WriteLine("No neighbour found at " + neighbour);
            } else {
                if (this.neighbourSteps[neighbour].ContainsKey(port))
                    this.neighbourSteps[neighbour][port] = steps;
                else
                    this.neighbourSteps[neighbour].Add(port, steps);
            }
        }

        public void Recompute(int port) {
            int rSteps, rNb;
            if (this.homePort == port) {
                rSteps = 0; rNb = homePort;
            } else {
                // TODO opvragen
            }
            // this.routes[port] = new int[2] { rSteps, rNb };
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
        public Connection Connection(int neighbourPort) {
            Connection res = null;
            this.neighbourConnections.TryGetValue(neighbourPort, out res);
            return res;
        }

        public void purgeNeighbour(int neighbourPort) {
            this.neighbourConnections.Remove(neighbourPort);
            this.neighbourSteps.Remove(neighbourPort);
        }

        public void purgeNode(int port) {
            if (this.neighbourConnections.ContainsKey(port)) this.neighbourConnections.Remove(port);
            if (this.neighbourSteps.ContainsKey(port)) this.neighbourSteps.Remove(port);
            if (this.routes.ContainsKey(port)) this.routes.Remove(port);
        }
    }
}
