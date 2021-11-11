using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ServerConnection
    {
        static X509Certificate2 serverCertificate = null;
        public ClientProcessing menager { get; set; }


        public void RunServer() {
            serverCertificate = new X509Certificate2("/home/put-inf-7-od/cert.pfx", "+kTQ2U_MG[((3}dM", X509KeyStorageFlags.MachineKeySet);
            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            TcpListener listener = new TcpListener(IPAddress.Any, 13579);
            listener.Start();
            while (true) {
                TcpClient client = listener.AcceptTcpClient();
                Task.Run(() => { ClientConnectionAsync(client); });
            }
        }


        /// <summary>
        /// Used to request-response type of communication like: 
        /// Login, Registry, Start new conversation, End conversation, Get Friends
        /// </summary>
        /// <param name="obj"></param>
        public async void ClientConnectionAsync(Object obj) {
            TcpClient client = obj as TcpClient;
            SslStream stream = new SslStream(
                client.GetStream(), false);

            stream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: false);

            int clientId = menager.AddActiveUser();
            byte[] message;
            Decoder decoder = Encoding.ASCII.GetDecoder();
            while (true) {
                try {
                    //Read message
                    string sendMessage = "";
                    //Utworzenie bufora do przechowywania otrzymanych od klienta danych
                    byte[] buffer = new byte[2048];
                    StringBuilder messageData = new StringBuilder();
                    //Informacja o liczbie odebranych bajtów
                    int bytes = -1;
                    do {
                        //Oczekiwanie na wiadomość od klienta
                        bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                        //Zdekodowanie otrzymanych bajtów na tekst
                        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                        decoder.GetChars(buffer, 0, bytes, chars, 0);
                        messageData.Append(chars);
                        if (messageData.ToString().IndexOf("$$$") != -1) break;
                    } while (bytes != 0);

                    //Prepare response
                    sendMessage = menager.ProccesClient(messageData.ToString(), clientId);

                    //Disconnection
                    if (sendMessage == "") {
                        message = Encoding.ASCII.GetBytes("Error:0$$$");
                        stream.Write(message);
                        Thread.Sleep(1000);
                        break;
                    }

                    message = Encoding.ASCII.GetBytes(sendMessage);
                    //Send response
                    stream.Write(message);
                }
                catch (Exception e) {
                    menager.Disconnect("", clientId);
                    Console.WriteLine(e.Message);
                    break;
                }

            }
        }


        public ServerConnection() {
            menager = new ClientProcessing();
            RunServer();
        }
    }
}
