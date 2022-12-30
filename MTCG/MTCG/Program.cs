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
            Console.WriteLine("Starting server on port 10001");
            Server.Server server = new Server.Server(10001);
            try
            {
                server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }
        }
    }
}