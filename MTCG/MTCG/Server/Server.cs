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

        public void Start()
        {
            _listener.Start(5);

            while (true)
            {
                try
                {
                    Console.WriteLine("Waiting for connection...");

                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    ThreadPool.QueueUserWorkItem(HandleClient, client);

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
