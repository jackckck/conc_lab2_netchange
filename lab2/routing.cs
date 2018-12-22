using System;
using System.Linq;
using System.Collections.Generic;

namespace lab2 {
    public class RoutingTable {
        private readonly int homePort;
        private readonly int nodeCount;
        public readonly object neighbourConnectionsLock;
        public readonly object neighbourDistancesLock;
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
            this.neighbourConnectionsLock = new object();
            this.neighbourDistancesLock = new object();
            this.routesLock = new object();
            //this.nodeCount = 1 + neighbours.Length;
        }

        public override string ToString() {
            string res = "";
            lock (this.routesLock) foreach (KeyValuePair<int, int[]> route in this.routes) {
                    res += route.Key + " " + route.Value[0] + " ";
                    if (route.Key == this.homePort) res += "local";
                    else res += route.Value[1];
                    res += "\n";
                }

            return res;
        }

        public Dictionary<int, int[]> GetRoutes() {
            lock (this.routesLock) return this.routes;
        }

        // returns null if no route
        public Connection GetConnection(int farPort) {
            /*
            Console.WriteLine("// | R LOOKING FOR " + farPort);
            foreach (KeyValuePair<int, int[]> kvp in this.routes) {
                Console.WriteLine(string.Format("// | {0} {1} {2}", kvp.Key, kvp.Value[0], kvp.Value[1]));
            }
            Console.WriteLine("// | NC LOOKING FOR " + farPort);
            foreach (KeyValuePair<int, Connection> kvp in this.neighbourConnections) {
                Console.WriteLine(string.Format("// | {0} {1}", kvp.Key, kvp.Value));
            }
            */

            lock (this.routesLock) lock (this.neighbourConnectionsLock) {
                if (this.routes.TryGetValue(farPort, out int[] route) &&
                    this.neighbourConnections.TryGetValue(route[1], out Connection res))
                    return res;

                return null;
            }
        }

        // get all the neighbour connections
        public Connection[] GetNeighbourConnections() {
            lock (this.neighbourConnectionsLock) return neighbourConnections.Values.ToArray();
        }

        // add a neighbour
        public List<int[]> AddNeighbour(int neighbourPort, Connection connection) {
            lock (this.neighbourConnectionsLock) lock (this.neighbourDistancesLock) {
                this.neighbourConnections[neighbourPort] = connection;
                this.neighbourDistances[neighbourPort] = new Dictionary<int, int>();
                this.routes[neighbourPort] = new int[2] { 1, neighbourPort };

                
                Console.WriteLine("// | R BEFORE");
                foreach (KeyValuePair<int, int[]> kvp in this.routes) {
                    Console.WriteLine(string.Format("// | {0} {1} {2}", kvp.Key, kvp.Value[0], kvp.Value[1]));
                }
                
                return RecomputeAll();
            }
        }

        // remove a connection
        public List<int[]> RemoveNeighbour(int neighbourPort) {
            lock (this.neighbourConnectionsLock) lock (this.neighbourDistancesLock) {
                this.neighbourConnections.Remove(neighbourPort);
                this.neighbourDistances.Remove(neighbourPort);

                return RecomputeAll();
            }
        }

        // updates the routing table's knowledge of the distance between neighbourPort and destinationPort
        public bool UpdateNeighbourDistance(int neighbourPort, int farPort, int newDistance) {
            lock (this.neighbourDistancesLock) {
                if (this.neighbourDistances.TryGetValue(neighbourPort, out Dictionary<int, int> distances)) {
                    distances[farPort] = newDistance;
                    this.neighbourDistances[neighbourPort] = distances;
                }
                return Recompute(farPort);
            }
        }

        private bool Recompute(int farPort) {
            Console.WriteLine("// Recompute op route " + farPort);
            int[] newRoute = new int[2];

            lock (this.routesLock) {
                if (this.homePort == farPort) {
                    this.routes[farPort] = new int[2] { 0, homePort };
                    return false;
                }

                // compute new route
                // int lowestDistance = this.nodeCount;
                int lowestDistance = 20;
                lock (this.neighbourDistancesLock) foreach (KeyValuePair<int, Dictionary<int, int>> neighbourDistance in this.neighbourDistances) {
                    // Console.WriteLine(neighbourDistance.Key);
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

                Console.WriteLine("// | R AFTER");
                foreach (KeyValuePair<int, int[]> kvp in this.routes) {
                    Console.WriteLine(string.Format("// | {0} {1} {2}", kvp.Key, kvp.Value[0], kvp.Value[1]));
                }

                return res;
            }
        }

        // recomputes routes and a returns list of routes that have been updated
        private List<int[]> RecomputeAll() {
            List<int[]> updatedRoutes = new List<int[]>();

            lock (this.routesLock) {
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
 