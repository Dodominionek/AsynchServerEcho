﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ServerEchoLibrary
{
    public class AsyncServerEcho : AbstractServerEcho
    {
        byte[] welcomeMessage;
        byte[] continueMessage;
        byte[] goodMessage;
        byte[] badMessage;
        byte[] message;

        public delegate void TransmissionDataDelegate(NetworkStream stream);
        public AsyncServerEcho(IPAddress IP, int port) : base(IP, port)
        {
            this.welcomeMessage = new ASCIIEncoding().GetBytes("Dzien dobry! Podaj login, a nastepnie haslo. ");
            this.continueMessage = new ASCIIEncoding().GetBytes("Aby kontynuowac podaj login, a nastepnie haslo. ");
            this.goodMessage = new ASCIIEncoding().GetBytes("Dobry login i haslo! ");
            this.badMessage = new ASCIIEncoding().GetBytes("Bledny login lub haslo! Prosze sprobowac ponownie. ");
        }
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);
                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);
                //while (!result.IsCompleted);
            }
        }

        private void TransmissionCallback(IAsyncResult ar)
        {
        }
        protected override void BeginDataTransmission(NetworkStream stream)
        {
            byte[] buffer = new byte[Buffer_size];
            stream.Write(welcomeMessage, 0, welcomeMessage.Length);
            while (true)
            {
                try
                {
                    int responseLength = stream.Read(buffer, 0, buffer.Length);
                    if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                    {
                        responseLength = stream.Read(buffer, 0, buffer.Length);
                    }
                    stream.Write(buffer, 0, buffer.Length);
                    var login = Encoding.UTF8.GetString(buffer, 0, responseLength);
                    responseLength = stream.Read(buffer, 0, buffer.Length);
                    if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                    {
                        responseLength = stream.Read(buffer, 0, buffer.Length);
                    }
                    stream.Write(buffer, 0, buffer.Length);
                    var password = Encoding.UTF8.GetString(buffer, 0, responseLength);
                    if(login == "login" && password == "password")
                    {
                        message = new ASCIIEncoding().GetBytes("Prosze podac imie, a ja odpowiem czy to imie chlopca czy dziewczynki. ");
                        stream.Write(goodMessage, 0, goodMessage.Length);
                        stream.Write(message, 0, message.Length);
                        responseLength = stream.Read(buffer, 0, buffer.Length);
                        if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                        {
                            responseLength = stream.Read(buffer, 0, buffer.Length);
                        }
                        Check(buffer, responseLength);
                        stream.Write(message, 0, message.Length);
                        stream.Write(continueMessage, 0, continueMessage.Length);
                    }
                    else
                    {
                        stream.Write(badMessage, 0, badMessage.Length);
                        if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                        {
                            responseLength = stream.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

        public void Check(byte[] ClientMessage, int len)
        {
            String Mess = Encoding.UTF8.GetString(ClientMessage, 0, len).Trim();
            char last = '\0';
            bool check = true;
            if (Mess[0] == '\0') check = false;
            last = Mess[Mess.Length - 1];
            if (last == 'a' && check == true)
            {
                message = new ASCIIEncoding().GetBytes("To imie dziewczynki ");
            }
            else if (check == true)
            {
                message = new ASCIIEncoding().GetBytes("To imie chlopca ");
            }
        }

        public override void Start()
        {
            StartListening();
            AcceptClient();
        }
    }
}

