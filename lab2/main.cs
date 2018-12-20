using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace lab2 {
    public class Program {
        static void Main(string[] args) {
            // create a new instance with given args
            new Node(args);
        }
    }

    public class Node {
        public int port;
        private RoutingTable routing;

        // constructor
        public Node(string[] ports) {
            Console.Title = "NetChange " + ports[0];

            // list all neighbour ports
            int[] neighbourPorts = new int[ports.Length - 1];
            for (int i = 1; i < ports.Length; i++) neighbourPorts[i - 1] = int.Parse(ports[i]);

            // instanciate routing table
            this.routing = new RoutingTable(port, neighbourPorts);

            // get own port and start listening
            this.port = Int32.Parse(ports[0]);
            TcpListener server = new TcpListener(IPAddress.Any, this.port);
            server.Start();
            new Thread(() => ListenConnection(server)).Start();

            // create connection with all neighbours
            foreach (int neighbourPort in neighbourPorts) {
                // smallest portnumber requests connection
                if (this.port < neighbourPort) AddConnection(neighbourPort);
            }
        }

        // continiously accept new connections
        private void ListenConnection(TcpListener handle) {
            loop:

            // create readers for new connection
            TcpClient newNeighbour = handle.AcceptTcpClient();
            StreamReader neighbourIn = new StreamReader(newNeighbour.GetStream());
            StreamWriter neighbourOut = new StreamWriter(newNeighbour.GetStream()) { AutoFlush = true };

            // read connecting port
            int neighbourPort = int.Parse(neighbourIn.ReadLine());

            // add connection to routing table
            Connection connection = new Connection(neighbourIn, neighbourOut, ProcessMessage);
            this.routing.AddConnection(neighbourPort, connection);

            Console.WriteLine("Verbonden: " + neighbourPort);

            goto loop;
        }

        // send a message to a port
        private void Send(int farPort, string message) {
            routing.GetConnection(farPort).Send(message);
        }

        // process an incoming message
        private void ProcessMessage(string message) {
            switch (message[0]) {
                // new connection from neighbour 
                case 'c':

                    break;
                // connection from neighbour lost
                case 'b':

                    break;
                // 
                case 'd':

                    break;
                case 'm':

                    break;
            }
        }

        private void AddConnection(int neighbourPort) {
            // smallest portnumber requests connection
            this.routing.AddConnection(neighbourPort, new Connection(this.port, neighbourPort, ProcessMessage));
        }

        private void DestroyConnection(int neighbourPort) {
            // todo create
        }

        private void ShowTable() {
            Console.WriteLine(this.routing);
        }
    }
}