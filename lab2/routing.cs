using System;
using System.Linq;
using System.Collections.Generic;

namespace lab2 {
    public class RoutingTable {
        private readonly int homePort;
        private readonly int nodeCount;
        public readonly object routesLock;

        // key ^= neighbour's port int, value ^= Connection to neighbour
        private Dictionary<int, Connection> neighbourConnections = new Dictionary<int, Connection>();
        // key ^= neighbour's port int, value ^= dictionary for which: key ^= node, value ^= neighbour's distance to node
        private Dictionary<int, Dictionary<int, int>> neighbourDistances = new Dictionary<int, Dictionary<int, int>>();
        // if route = routes[key], then route[0] is the amount of steps, and route[1] the preferred neighbour
        private Dictionary<int, int[]> routes = new Dictionary<int, int[]>();

        public RoutingTable(int homePort) {
            this.homePort = homePort;
            this.routes[homePort] = new int[2] { 0, homePort };
            this.routesLock = new object();
            //this.nodeCount = 1 + neighbours.Length;
        }

        public override string ToString() {
            lock (this.routesLock) {
                string res = "";
                foreach (KeyValuePair<int, int[]> route in this.routes) {
                    res += route.Key + " " + route.Value[0] + " ";
                    if (route.Key == this.homePort) res += "local";
                    else res += route.Value[1];
                    res += "\n";
                }

                return res;
            }
        }

        public Dictionary<int, int[]> getRoutes() {
            lock (this.routesLock) return this.routes;
        }

        // returns null if no route
        public Connection GetConnection(int farPort) {
            lock (this.routesLock) {
                if (this.routes.TryGetValue(farPort, out int[] route) &&
                    this.neighbourConnections.TryGetValue(route[1], out Connection res))
                    return res;
                return null;
            }
        }

        // get all the neighbour connections
        public Connection[] GetNeighbourConnections() {
            return neighbourConnections.Values.ToArray();
        }

        // add a neighbour
        public List<int[]> AddNeighbour(int neighbourPort, Connection c) {
            this.neighbourConnections[neighbourPort] = c;
            this.neighbourDistances[neighbourPort] = new Dictionary<int, int>();

            return RecomputeAll();
        }

        // remove a connection
        public List<int[]> RemoveNeighbour(int neighbourPort) {
            this.neighbourConnections.Remove(neighbourPort);
            this.neighbourDistances.Remove(neighbourPort);

            return RecomputeAll();
        }

        // updates the routing table's knowledge of the distance between neighbourPort and destinationPort
        public bool UpdateNeighbourDistance(int neighbourPort, int farPort, int newDistance) {
            if (this.neighbourDistances.TryGetValue(neighbourPort, out Dictionary<int, int> distances)) distances[farPort] = newDistance;
            return Recompute(farPort);
        }

        private bool Recompute(int farPort) {
            lock (this.routesLock) {
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

                // true if no update is necessary
                bool res = this.routes.TryGetValue(farPort, out int[] route) && route == newRoute;
                this.routes[farPort] = newRoute;
                return res;
            }
        }

        // recomputes routes and a returns list of routes that have been updated
        private List<int[]> RecomputeAll() {
            lock (this.routesLock) {
                List<int[]> updatedRoutes = new List<int[]>();
                int[] portRoutes = this.routes.Keys.ToArray();
                foreach (int route in portRoutes) {
                    // deze code is een beetje lelijk, maar wel geheel
                    // als recompute leidt tot een verandering
                    if (Recompute(route)) {
                        // voeg updated route toe aan returnlijst
                        if (routes.TryGetValue(route, out int[] newRoute)) {
                            newRoute[1] = route;
                            updatedRoutes.Add(newRoute);
                        }
                    }
                }

                return updatedRoutes;
            }
        }
    }
}