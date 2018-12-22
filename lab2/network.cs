using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace lab2 {
    public class Connection {
        private readonly StreamReader reader;
        private readonly StreamWriter writer;
        private readonly Node node;

        // constructor for client
        public Connection(int port, int neighbourPort, Node node) {
            // initiate reader and writer
            TcpClient client = new TcpClient("localhost", neighbourPort);
            this.reader = new StreamReader(client.GetStream());
            this.writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            this.node = node;

            // tell neighbour our port
            Send(port.ToString());
            // this.node.AddConnection(neighbourPort);

            new Thread(Listen).Start();
        }

        // constructor for server
        public Connection(StreamReader read, StreamWriter write, Node node) {
            this.reader = read;
            this.writer = write;
            this.node = node;

            new Thread(Listen).Start();
        }

        // send message
        public void Send(string message) {
            this.writer.WriteLine(message);
        }

        // pass messages over to message processer.
        private void Listen() {
            try { while (true) this.node.ProcessMessage(this.reader.ReadLine()); }
            catch (IOException) { Console.WriteLine("Verbroken: " + -1); } // todo correct poortnummer
        }
    }
}
