using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace lab2 {
    public class Connection {
        private StreamReader Reader;
        private StreamWriter Writer;

        // constructor for client
        public Connection(int port, int neighbourPort) {
            // initiate reader and writer
            TcpClient client = new TcpClient("localhost", neighbourPort);
            this.Reader = new StreamReader(client.GetStream());
            this.Writer = new StreamWriter(client.GetStream()) { AutoFlush = true };

            // tell neighbour our port
            Send(port.ToString());

            new Thread(Listen).Start();
        }

        // constructor for server
        public Connection(StreamReader read, StreamWriter write) {
            this.Reader = read;
            this.Writer = write;

            new Thread(Listen).Start();
        }

        public void Send(string message) {
            this.Writer.Write(message);
        }
        
        private void Listen() {
            string input;

            try {
                Loop:

                input = Reader.ReadLine();
                switch (input[0]) {
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

                goto Loop;
            }
            catch { } // todo print verbroken
        }
    }
}
