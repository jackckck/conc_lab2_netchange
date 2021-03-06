﻿using System;
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
        private readonly int port;
        private readonly RoutingTable routing;

        // constructor
        public Node(string[] ports) {
            Console.Title = "NetChange " + ports[0];

            // instanciate routing table
            this.port = int.Parse(ports[0]);
            this.routing = new RoutingTable(this.port);

            // get own server and start listening
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
                        int neighbourPort = int.Parse(command[1]);
                        SendMessageToPort(neighbourPort, "D " + this.port);
                        RemoveConnection(neighbourPort);
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
                    if (farPort != this.port) {
                        SendMessageToPort(farPort, message);
                        Console.WriteLine(string.Format("Bericht voor {0} doorgestuurd naar {1}", farPort, -1)); // todo correct poortnummer
                    }
                    // todo fix message splitting
                    else Console.WriteLine(command[2]);
                    break;
                // connection from neighbour closed
                case "D":
                    RemoveConnection(int.Parse(command[1]));
                    break;
                // distance update from neighbour
                case "U":
                    UpdateConnection(int.Parse(command[1]), int.Parse(command[2]), int.Parse(command[3]));
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
            Connection connection = this.routing.GetConnection(farPort);
            if (connection != null) {
                Console.WriteLine(string.Format("// {0} heeft poort {1} een bericht gestuurd: {2}", this.port, farPort, message));
                connection.Send(message);
            } else Console.WriteLine(string.Format("// {0} kan poort {1} niet bereiken ({2})", this.port, farPort, message));
        }
        // send a message to all neighbours
        private void SendMessageToNeighbours(string message) {
            lock (this.routing.neighbourConnectionsLock) foreach (Connection connection in this.routing.GetNeighbourConnections()) connection.Send(message);
        }

        // add a connection
        public void AddConnection(int neighbourPort) {
            AddConnection(neighbourPort, new Connection(this.port, neighbourPort, this));
        }
        public void AddConnection(int neighbourPort, Connection connection) {
            // lock (this.routing.routesLock) Console.WriteLine(string.Format("// Added? {0} {1}", this.routing.GetRoutes().ContainsKey(neighbourPort), this.routing.GetConnection(neighbourPort) != null));
            // supply new connection with all known routes
            lock (this.routing.routesLock) foreach (int[] route in this.routing.AddNeighbour(neighbourPort, connection)) SendMessageToPort(neighbourPort, string.Format("U {0} {1} {2}", this.port, route[1], route[0]));
            // notify neighbours of updated routes
            SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, neighbourPort, 1));
        }
        // update a connection
        private void UpdateConnection(int farPort1, int farPort2, int distance) {
            Console.WriteLine(string.Format("// Connection update: {0} {1} {2}", farPort1, farPort2, distance));
            if (this.routing.UpdateNeighbourDistance(farPort1, farPort2, distance)) lock (this.routing.routesLock) SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, farPort2, this.routing.GetRoutes()[farPort2][0]));
        }
        // remove a connection
        private void RemoveConnection(int neighbourPort) {
            foreach (int[] route in this.routing.RemoveNeighbour(neighbourPort)) SendMessageToNeighbours(string.Format("U {0} {1} {2}", this.port, route[1], route[0]));
        }
    }
}