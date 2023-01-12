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
            /*Console.WriteLine("Starting server on port 10001");
            Server.Server server = new Server.Server(10001);
            try
            {
                server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }*/
            Console.WriteLine("Database test:");
            var db = new Database();
            db.ExecuteQuery("INSERT INTO mytable (col1, col2) VALUES ('value1', 'value2')");
        }
    }
}