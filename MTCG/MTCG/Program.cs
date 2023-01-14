using System;
using System.Net.Sockets;
using System.Net;
using MTCG.DB;
using Npgsql;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on port 10001");
            var db = new Database();
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