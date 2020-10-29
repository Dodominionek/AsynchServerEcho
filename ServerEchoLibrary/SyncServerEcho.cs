using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerEchoLibrary
{
    /// <summary>
    /// This class implements the most basic TCP Server of the Echo Type.
    /// </summary>
    public class SyncServerEcho : AbstractServerEcho
    {
        public SyncServerEcho(IPAddress IP, int port) : base(IP, port)
        {

        }
        protected override void AcceptClient()
        {
            TcpClient = TcpListener.AcceptTcpClient();
            byte[] buffer = new byte[Buffer_size];
            Stream = TcpClient.GetStream();
            BeginDataTransmission(Stream);
        }
        protected override void BeginDataTransmission(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int message_size = Stream.Read(buffer, 0, Buffer_size);
                    Stream.Write(buffer, 0, message_size);
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Overrided comment.
        /// </summary>
        public override void Start()
        {
            StartListening();
            //transmission starts within the accept function
            AcceptClient();
        }
    }
}