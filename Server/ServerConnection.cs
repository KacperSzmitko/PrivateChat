﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ServerConnection
    {
        public ClientProcessing menager { get; set; }
        public void RunServer()
        {
            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            TcpListener listener = new TcpListener(IPAddress.Any, 17777);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Task.Run(() => { ClientConnectionAsync(client); });
            }
        }
        /// <summary>
        /// Used to request-response type of communication like: 
        /// Login, Registry, Start new conversation, End conversation, Get Friends
        /// </summary>
        /// <param name="obj"></param>
        public async void ClientConnectionAsync(Object obj)
        {
            TcpClient client = obj as TcpClient;
            NetworkStream stream = client.GetStream();

            byte[] message;

            //Task.Run(() => { ServerMessagesAsync(stream, clientID); });
            while (true)
            {
                try
                {
                    //Read message
                    string sendMessage = "";
                    byte[] buffer = new byte[2048];
                    StringBuilder messageData = new StringBuilder();
                    int bytes = -1;

                    bytes = await stream.ReadAsync(buffer, 0, buffer.Length);

                    //Decode message
                    Decoder decoder = Encoding.ASCII.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    messageData.Append(chars);

                    //Prepare response
                    sendMessage = "";

                    //Disconnection
                    if (sendMessage == "")
                    {
                        message = Encoding.ASCII.GetBytes("Response:0$$");
                        stream.Write(message);
                        Thread.Sleep(2000);
                        break;
                    }
                    message = Encoding.ASCII.GetBytes(sendMessage);

                    //Send response
                    stream.Write(message);
                }
                catch
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Used in async comunication like:
        /// Revice message from another user
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="clientID"></param>
        public async void ServerMessagesAsync(Object obj, int clientID)
        {
            NetworkStream stream = obj as NetworkStream;
            string sendMessage = "";
            byte[] message;
            while (true)
            {
                try
                {
                    //Prepare response
                    sendMessage = "";

                    //Disconnection
                    if (sendMessage == "")
                    {
                        message = Encoding.ASCII.GetBytes("Response:0$$");
                        stream.Write(message);
                        Thread.Sleep(2000);
                        break;
                    }
                    message = Encoding.ASCII.GetBytes(sendMessage);

                    //Send response
                    stream.Write(message);
                }
                catch
                {
                    return;
                }
            }
        }


        public ServerConnection()
        {
            menager = new ClientProcessing();
        }
    }
}