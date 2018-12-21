using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace lab2 {
    public class Program {
        static void Main(string[] args) {
            // create a new instance with given args
            new Node(args);
        }
    }

    public class Node {
        private int port;
        private RoutingTable routing;

        // constructor
        public Node(string[] ports) {
            Console.Title = "NetChange " + ports[0];

            // instanciate routing table
            this.routing = new RoutingTable(port);

            // get own port and start listening
            this.port = int.Parse(ports[0]);
            TcpListener server = new TcpListener(IPAddress.Any, this.port);
            server.Start();
            new Thread(() => ListenConnection(server)).Start();

            // create connection with all neighbours
            int neighbourPort;
            foreach (string port in ports) {
                neighbourPort = int.Parse(port);
                // smallest portnumber requests connection
                if (this.port < neighbourPort) AddConnection(neighbourPort);
            }

            // listen on the console
            ListenConsole();
        }

        #region string processing
        // continiously accept new connections
        private void ListenConnection(TcpListener handle) {
            while (true) {
                // create readers for new connection
                TcpClient newNeighbour = handle.AcceptTcpClient();
                StreamReader neighbourIn = new StreamReader(newNeighbour.GetStream());
                StreamWriter neighbourOut = new StreamWriter(newNeighbour.GetStream()) { AutoFlush = true };

                // read connecting port
                int neighbourPort = int.Parse(neighbourIn.ReadLine());

                // add connection to routing table
                Connection connection = new Connection(neighbourIn, neighbourOut, this);
                AddConnection(neighbourPort, connection);
                Console.WriteLine("Verbonden: " + neighbourPort);
            }
        }

        // continiously read the console
        private void ListenConsole() {
            while (true) {
                string[] command = Console.ReadLine().Split(' ');
                switch (command[0]) {
                    // print table
                    case "R":
                        PrintTable();
                        break;
                    // send message
                    case "B":
                        // todo fix message splitting
                        SendMessageToPort(int.Parse(command[1]), command[2]);
                        break;
                    // make connection
                    case "C":
                        AddConnection(int.Parse(command[1]));
                        break;
                    // destroy connection
                    case "D":
                        RemoveConnection(int.Parse(command[1]));
                        break;
                }
            }
        }

        // process an incoming message
        public void ProcessMessage(string message) {
            string[] command = message.Split(' ');

            switch (command[0]) {
                // message from neighbour
                case "B":
                    // forward the message if it's meant for someone else
                    int farPort = int.Parse(command[1]);
                    if (farPort != this.port) SendMessageToPort(farPort, message);
                    // todo fix message splitting
                    else Console.WriteLine(command[2]);
                    break;
                // connection from neighbour closed
                case "D":
                    int neighbourPort = int.Parse(command[1]);
                    if (this.routing.RemoveNeighbour(neighbourPort)) {
                        // todo get distance
                        SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, neighbourPort, "???"));
                    }
                    break;
                // distance update from neighbour
                case "U":
                    if (this.routing.UpdateNeighbourDistance(int.Parse(command[1]), int.Parse(command[2]), int.Parse(command[3]))) {
                        SendMessageToNeighbours(message);
                    }
                    break;
            }
        }
        #endregion

        // print the table
        private void PrintTable() {
            Console.WriteLine(this.routing);
        }

        // send a message to a port
        private void SendMessageToPort(int farPort, string message) {
            this.routing.GetConnection(farPort).Send(message);
        }

        // send a message to all neighbours
        private void SendMessageToNeighbours(string message) {
            foreach (Connection connection in this.routing.GetNeighbourConnections()) connection.Send(message);
        }

        // add a connection
        private void AddConnection(int neighbourPort) {
            this.routing.AddNeighbour(neighbourPort, new Connection(this.port, neighbourPort, this));
            InformNeighbours(neighbourPort);
        }
        private void AddConnection(int neighbourPort, Connection connection) {
            this.routing.AddNeighbour(neighbourPort, connection);
            InformNeighbours(neighbourPort);
        }
        private void InformNeighbours(int neighbourPort) {
            foreach (KeyValuePair<int, int[]> route in this.routing.getRoutes()) SendMessageToPort(neighbourPort, string.Format("U {0} {1} {2}", this.port, route.Key, route.Value[0]));
            SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, neighbourPort, 1));
        }

        // remove a connection
        private void RemoveConnection(int neighbourPort) {
            this.routing.RemoveNeighbour(neighbourPort);
            // todo get distance
            SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, neighbourPort, "???"));
        }
    }
}