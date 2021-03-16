using System.Text;
using System.Net.Sockets;

namespace Client 
{
    public class ServerConnection 
    {

        private const int bufferSize = 1024;

        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;

        public bool DataAvailable {
            get { return stream.DataAvailable; }
        }

        public ServerConnection(string serverAddress, ushort serverPort) {
            this.tcpClient = new TcpClient(serverAddress, serverPort);
            this.stream = tcpClient.GetStream();
        }

        public void SendMessage(string message) {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            this.stream.Write(messageBytes);
            this.stream.Flush();
        }

        public string ReadMessage() {
            byte[] buffer = new byte[bufferSize];
            Decoder decoder = Encoding.ASCII.GetDecoder();
            string messageString = "";

            do {
                int bytesRead = stream.Read(buffer, 0, bufferSize);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytesRead)];
                decoder.GetChars(buffer, 0, bytesRead, chars, 0);
                messageString += new string(chars);
            } while (stream.DataAvailable);

            return messageString;
        }

        public void CloseConnection() {
            this.stream.Close();
            this.tcpClient.Close(); 
        }

    }
}
