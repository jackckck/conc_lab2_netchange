using System;
using System.Threading;
using System.Collections.Generic;

namespace lab2 {
    public class Program {
        static void Main(string[] args) {
            // create a new instance with given args
            new Instance(args);
        }
    }

    public class Instance {
        public int port;
        private int[] neighbourPorts;
        private RoutingTable routing;

        public Instance(string[] ports) {
            // get own port and start listening
            Console.Title = "NetChange " + ports[0];
            this.port = Int32.Parse(ports[0]);
            new Listener(this.port);

            // list all neighbour ports
            this.neighbourPorts = new int[ports.Length - 1];
            for (int i = 1; i < ports.Length; i++) this.neighbourPorts[i - 1] = Int32.Parse(ports[i]);

            // instanciate routing table
            this.routing = new RoutingTable(port, neighbourPorts);
        }

        private void ShowTable() {
            // todo
        }

        private void Send() {
            // todo
        }

        private void CreateConnection() {
            // todo
        }

        private void DestroyConnection() {
            // todo
        }
    }
}