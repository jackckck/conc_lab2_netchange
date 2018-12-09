using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace lab2 {
    public class Program {
        static void Main(string[] args) {
            new NetChange(args);
        }
    }

    public class NetChange {
        private int myPort;
        private int[] neighbourPorts;

        public NetChange(string[] ports) {
            this.myPort = Int32.Parse(ports[0]);

            this.neighbourPorts = new int[ports.Length - 1];
            for (int i = 1; i < ports.Length; i++)
                this.neighbourPorts[i - 1] = Int32.Parse(ports[i]);

        }
        public NetChange (int[] ports) {
            this.myPort = ports[0];

            this.neighbourPorts = new int[ports.Length - 1];
            for (int i = 1; i < ports.Length; i++)
                this.neighbourPorts[i - 1] = ports[i];
        }
    }
}