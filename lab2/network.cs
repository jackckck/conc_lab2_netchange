using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace lab2 {
    public class Connection {
        private StreamReader Reader;
        private StreamWriter Writer;
        private Action<string> ProcessMessage;

        // constructor for client
        public Connection(int port, int neighbourPort, Action<string> ProcessMessage) {
            // initiate reader and writer
            TcpClient client = new TcpClient("localhost", neighbourPort);
            this.Reader = new StreamReader(client.GetStream());
            this.Writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            this.ProcessMessage = ProcessMessage;

            // tell neighbour our port
            Send(port.ToString());

            new Thread(Listen).Start();
        }

        // constructor for server
        public Connection(StreamReader read, StreamWriter write, Action<string> ProcessMessage) {
            this.Reader = read;
            this.Writer = write;
            this.ProcessMessage = ProcessMessage;

            new Thread(Listen).Start();
        }

        // send message
        public void Send(string message) {
            this.Writer.Write(message);
        }

        // pass messages over to message processer
        private void Listen() {
            try { while (true) this.ProcessMessage(this.Reader.ReadLine()); }
            catch (Exception e) { Console.WriteLine("error x d " + e); } // todo print verbroken
        }
    }
}
