using System;
using System.Net.Sockets;
using System.Net;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "title";
            Console.WriteLine("Starting server on port 8080");
            Server.Server server = new Server.Server();
            server.Start();
        }
    }
}