using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerEchoLibrary
{
    /// <summary>
    /// This is class for Asynchronous Echo Server.
    /// </summary>
    public class AsynchServerEcho : AbstractServerEcho
    {
        #region Fields
        byte[] welcomeMessage;
        byte[] continueMessage;
        byte[] log;
        byte[] pass;
        byte[] space;
        byte[] goodMessage;
        byte[] badMessage;
        byte[] message;
        #endregion

        public delegate void TransmissionDataDelegate(NetworkStream stream);

        #region Properties
        /// <summary>
        /// Constructor for Server. Assigns and encodes messages for client.
        /// </summary>
        public AsynchServerEcho(IPAddress IP, int port) : base(IP, port)
        {
            this.welcomeMessage = new ASCIIEncoding().GetBytes("Dzien dobry! Podaj login, a nastepnie haslo. Zatwierdz je za pomoca klawisza ENTER. ");
            this.log = new ASCIIEncoding().GetBytes("wpisany login: ");
            this.pass = new ASCIIEncoding().GetBytes("wpisane haslo: ");
            this.space = new ASCIIEncoding().GetBytes(" ");
            this.continueMessage = new ASCIIEncoding().GetBytes("Aby kontynuowac podaj login, a nastepnie haslo. ");
            this.goodMessage = new ASCIIEncoding().GetBytes("Dobry login i haslo! ");
            this.badMessage = new ASCIIEncoding().GetBytes("Bledny login lub haslo! Prosze sprobowac ponownie. ");
        }
        #endregion

        #region Functions
        /// <summary>
        /// Function, which accepts new Client and creates new Delegate for him.
        /// </summary>
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

        /// <summary>
        /// Function, which controls communication with client and controls receiving and sending messages.
        /// </summary>
        protected override void BeginDataTransmission(NetworkStream stream)
        {
            byte[] buffer = new byte[Buffer_size];
            var space = new ASCIIEncoding().GetBytes(" ");
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
                    stream.Write(log, 0, log.Length);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Write(space, 0, space.Length);
                    var login = Encoding.UTF8.GetString(buffer, 0, responseLength);
                    buffer = new byte[Buffer_size];
                    responseLength = stream.Read(buffer, 0, buffer.Length);
                    if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                    {
                        responseLength = stream.Read(buffer, 0, buffer.Length);
                    }
                    stream.Write(pass, 0, pass.Length);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Write(space, 0, space.Length);
                    var password = Encoding.UTF8.GetString(buffer, 0, responseLength);
                    buffer = new byte[Buffer_size];
                    string pathLogPas = System.IO.Directory.GetCurrentDirectory();
                    string fileLogin = File.ReadAllText(pathLogPas + @"\login.txt");
                    string filePassword = File.ReadAllText(pathLogPas + @"\password.txt");
                    fileLogin = Regex.Replace(fileLogin, @"\t|\n|\r", "");
                    filePassword = Regex.Replace(filePassword, @"\t|\n|\r", "");
                    if (login == fileLogin && password == filePassword)
                    {
                        message = new ASCIIEncoding().GetBytes("Prosze podac imie, a ja odpowiem czy to imie chlopca czy dziewczynki. Zatwierdz imie za pomoca klawisza ENTER. ");
                        stream.Write(goodMessage, 0, goodMessage.Length);
                        stream.Write(message, 0, message.Length);
                        responseLength = stream.Read(buffer, 0, buffer.Length);
                        if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                        {
                            responseLength = stream.Read(buffer, 0, buffer.Length);
                        }
                        Check(buffer, responseLength);
                        writeToFile(Encoding.UTF8.GetString(message, 0, message.Length));
                        stream.Write(message, 0, message.Length);
                        stream.Write(continueMessage, 0, continueMessage.Length);
                        buffer = new byte[Buffer_size];
                    }
                    else
                    {
                        stream.Write(badMessage, 0, badMessage.Length);
                        if (Encoding.UTF8.GetString(buffer, 0, responseLength) == "\r\n")
                        {
                            responseLength = stream.Read(buffer, 0, buffer.Length);
                        }
                        buffer = new byte[Buffer_size];
                    }
                }
                catch (IOException e)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Function, which checks if name sent by client is male or female.
        /// </summary>
        public void Check(byte[] ClientMessage, int len)
        {
            String Mess = Encoding.UTF8.GetString(ClientMessage, 0, len).Trim();
            char last = '\0';
            bool check = true;
            if (Mess[0] == '\0') check = false;
            last = Mess[Mess.Length - 1];
            if (last == 'a' && check == true)
            {
                message = new ASCIIEncoding().GetBytes(Mess + ": To imie dziewczynki! ");
            }
            else if (check == true)
            {
                message = new ASCIIEncoding().GetBytes(Mess + ": To imie chlopca! ");
            }
        }

        /// <summary>
        /// Function, which starts Server, asks administrator for login and password for session and assigns it for session.
        /// </summary>
        public override void Start()
        {
            Console.WriteLine("Prosze podac login: ");
            string log = Console.ReadLine();
            Console.WriteLine("Prosze podac haslo: ");
            string pass = Console.ReadLine();
            Console.WriteLine("Wprowadzony login: " + log);
            Console.WriteLine("Wprowadzone haslo: " + pass);
            pass = Regex.Replace(pass, @"\t|\n|\r", "");
            log = Regex.Replace(log, @"\t|\n|\r", "");
            createFiles(log, pass);
            StartListening();
            AcceptClient();
        }

        /// <summary>
        /// Function, which creates files with results, login and password.
        /// </summary>
        public void createFiles(string log, string pass)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string filename = path + @"\results.txt";
            if (File.Exists(filename))
            {
            }
            using (FileStream fs = File.Create(filename))
            {
            }
            filename = path + @"\login.txt";
            if (File.Exists(filename))
            {
                File.Delete(filename);
                using (FileStream fs = File.Create(filename))
                {
                }
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
                {
                    file.WriteLine(log);
                }
            }
            else
            {
                using (FileStream fs = File.Create(filename))
                {
                }
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
                {
                    file.WriteLine(log);
                }
            }
            filename = path + @"\password.txt";
            if (File.Exists(filename))
            {
                File.Delete(filename);
                using (FileStream fs = File.Create(filename))
                {
                }
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
                {
                    file.WriteLine(pass);
                }
            }
            else
            {
                using (FileStream fs = File.Create(filename))
                {
                }
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
                {
                    file.WriteLine(pass);
                }
            }
        }

        /// <summary>
        /// Function, which writes results of session for file.
        /// </summary>
        public void writeToFile(String text)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string filename = path + @"\results.txt";
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(text);
            }
        }
        #endregion
    }
}

