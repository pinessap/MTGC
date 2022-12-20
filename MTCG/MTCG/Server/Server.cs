using System.Net.Sockets;
using System.Net;


namespace MTCG.Server
{

    class Server
    {
        private TcpListener _listener;

        public Server()
        {
            _listener = new TcpListener(IPAddress.Any, 10001);
        }

        public void Start() //listen for clients and accept their connection
        {
            _listener.Start(5); //queue up to 5 connections

            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for connection...");

                    TcpClient client = _listener.AcceptTcpClient(); //accept connection request
                    Console.WriteLine("Connected!");
                    ThreadPool.QueueUserWorkItem(HandleClient, client); //execute method inside of a thread

                }
                catch (Exception e)
                {
                    Console.WriteLine("error: " + e.Message);
                }
            }
        }

        private void HandleClient(object obj)
        {
            
        }
    }

    
}
