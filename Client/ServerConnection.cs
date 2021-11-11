using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Client 
{
    public class ServerConnection 
    {

        private const int bufferSize = 1024;

        private readonly TcpClient tcpClient;
        private readonly SslStream stream;

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return true;
        }

        public ServerConnection(string serverAddress, ushort serverPort, string certServerName) {
            this.tcpClient = new TcpClient(serverAddress, serverPort);
            this.stream = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            stream.AuthenticateAsClient(certServerName);
        }

        public void SendMessage(string message) {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message + "$");
            this.stream.Write(messageBytes);
            this.stream.Flush();
        }

        public string ReadMessage() {
            byte[] buffer = new byte[bufferSize];
            Decoder decoder = Encoding.ASCII.GetDecoder();
            string messageString = "";
            int bytesRead = 0;

            do {
                bytesRead = stream.Read(buffer, 0, bufferSize);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytesRead)];
                decoder.GetChars(buffer, 0, bytesRead, chars, 0);
                messageString += new string(chars);
                if (messageString.IndexOf("$$$") != -1) break;
            } while (bytesRead != 0);

            return messageString.Substring(0, messageString.Length - 1);
        }

        public void CloseConnection() {
            this.stream.Close();
            this.tcpClient.Close(); 
        }

    }
}
