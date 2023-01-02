using System.Net.Sockets;
using System.Net;


namespace MTCG.Server
{

    public class Server
    {
        private bool running = false;
        private TcpListener listener;

        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start() //listen for clients and accept their connection
        {
            running = true;
            listener.Start(1); //queue up to 5 connections

            
            try {
                while (running)
                {
                    Console.WriteLine("Waiting for connection...");

                    TcpClient client = listener.AcceptTcpClient(); //accept connection request
                    Console.WriteLine("Connected!");
                    ThreadPool.QueueUserWorkItem(HandleClient, client); //execute method inside of a thread
                }
            }catch (Exception e)
            {
                    Console.WriteLine("error: " + e.Message);
                    running = false;
                    listener.Stop();
            }

            running = false;
            listener.Stop();

        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient) obj;
            StreamReader reader = new StreamReader(client.GetStream());
            string message = "";

            try
            {
                while (reader.Peek() != -1)
                {
                    message += (char)reader.Read();
                }

                Request request = Request.GetRequest(message);
                //Console.WriteLine("Request START: \n" + request);
                //Console.WriteLine("END");
                Response response = Response.GetResponse(request);
                //Console.WriteLine("Response: " + response.Body);

                StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                writer.Write(response.ResponseString());

                client.Close();


            } catch (Exception e)
            {
                Console.WriteLine("\nSERVER error: " + e.Message);
                client.Close();
            }

        }
    }

    
}
