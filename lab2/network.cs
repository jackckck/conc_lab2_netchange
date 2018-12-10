using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace lab2 {
    class Listener {
        public Listener(int port) {
            // listen on given port
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            // start a thread for accepting connections
            new Thread(() => CreateConnection(server)).Start();
        }

        private void CreateConnection(TcpListener handle) {
            loop:

            // create readers for new connection
            TcpClient newNeighbour = handle.AcceptTcpClient();
            StreamReader neighbourIn = new StreamReader(newNeighbour.GetStream());
            StreamWriter neighbourOut = new StreamWriter(newNeighbour.GetStream()) { AutoFlush = true };

            // read connecting port
            int neighbourPort = int.Parse(neighbourIn.ReadLine());

            // add connection to routing table
            Connection connection = new Connection(neighbourIn, neighbourOut);
            // todo

            Console.WriteLine("Verbonden: " + neighbourPort);

            goto loop;
        }
    }

    class Connection {
        public StreamReader Read;
        public StreamWriter Write;

        // constructor for client
        public Connection(int port, int neighbourPort) {
            // initiate reader and writer
            TcpClient client = new TcpClient("localhost", neighbourPort);
            this.Read = new StreamReader(client.GetStream());
            this.Write = new StreamWriter(client.GetStream()) { AutoFlush = true };

            // tell neighbour our port
            Write.WriteLine(port);

            new Thread(Reader).Start();
        }

        // constructor for server
        public Connection(StreamReader read, StreamWriter write) {
            this.Read = read;
            this.Write = write;

            new Thread(Reader).Start();
        }

        // print whatever is read
        public void Reader() {
            try { while (true) Console.WriteLine(Read.ReadLine()); }
            catch { } // todo print verbroken
        }
    }
}
