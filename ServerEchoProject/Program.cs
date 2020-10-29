using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ServerEchoLibrary;

namespace ServerEchoProject
{
    class Program
    {
        static void Main(string[] args)
        { 
            AsyncServerEcho server = new AsyncServerEcho(IPAddress.Parse("127.0.0.1"), 10000);
            server.Start();
        }
    }
}
