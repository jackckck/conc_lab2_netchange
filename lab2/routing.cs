using System;
using System.Collections.Generic;

namespace lab2 {
    public class RoutingTable {
        private int originPort;
        // if route = routes[key], then route[0] is the amount of steps, and route[1] the preferred neighbour
        private Dictionary<int, int[]> routes = new Dictionary<int, int[]>();
        // if route = foreignRoutes[key], then route is the amount of steps between ports key[0] and key[1]
        private Dictionary<int[], int> steps = new Dictionary<int[], int>();

        public RoutingTable(int origin, int[] neighbours) {
            this.originPort = origin;
            // routes to direct neighbours
            for (int i = 0; i < neighbours.Length; i++)
                this.routes.Add(neighbours[i], new int[2] { 1, neighbours[i] });
        }

        public int[] TryGetRoute(int port) {
            // no route ;(
            int[] res = new int[2] { -1, -1 };
            this.routes.TryGetValue(port, out res);
            return res;
        }

        public int[] TryGetSteps(int portA, int portB) {
            int res = -1;
            if (!this.steps.TryGetValue(new int[2] { portA, portB }, out res)) {
                this.steps.TryGetValue(new int[2] { portB, portA }, out res);
            }
            return res;
        }

        public override string ToString() {
            // todo implement
            return "todo";
        }
    }
}
