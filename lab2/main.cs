using System;
using System.Threading;
using System.Collections.Generic;

namespace lab2 {
    public class Program {
        static void Main(string[] args) {
            new Instance(args);
        }
    }

    public class Instance {
        private int myPort;
        private int[] neighbourPorts;

        public Instance(string[] ports) {
            // get own port
            this.myPort = Int32.Parse(ports[0]);

            // list all neighbour ports
            this.neighbourPorts = new int[ports.Length - 1];
            for (int i = 1; i < ports.Length; i++)
                this.neighbourPorts[i - 1] = Int32.Parse(ports[i]);
        }

        private void ShowTable() {

        }

        private void Send() {

        }

        private void CreateConnection() {

        }

        private void DestroyConnection() {

        }
    }
}