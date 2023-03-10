using System.Net.Sockets;
using System.Net;
using MTCG.DB;


namespace MTCG.Server
{

    public class Server
    {
        ServerMethods serverData = new ServerMethods(new Database());

        private bool running = false;
        private readonly TcpListener listener;

        public Server(int port) // constructor
        {
            listener = new TcpListener(IPAddress.Any, port); //instance of TcpListener
        }

        public void Start() //listen for clients and accept their connection
        {
            running = true;
            listener.Start(5); //queue up to 5 connections

            try 
            {
                while (running)
                {
                    Console.WriteLine("Waiting for connection...");
                    TcpClient client = listener.AcceptTcpClient(); //accept connection request
                    Console.WriteLine("Connected!");

                    ThreadPool.QueueUserWorkItem(HandleClient, new object[]{client, serverData}); //execute method inside of a thread with array of objects
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine("error: " + e.Message);
                    running = false;
                    listener.Stop();
            }

            running = false;
            listener.Stop();
        }

        private static void HandleClient(object obj) //get requests from client and send responses back
        {
            object[] objArr = obj as object[];

            TcpClient client = (TcpClient) objArr[0]; //cast object to TcpClient
            ServerMethods requestHandler = (ServerMethods) objArr[1]; //cast object to serverMethods

            StreamReader reader = new StreamReader(client.GetStream()); //reads characters from stream
            string message = "";

            try
            {
                while (reader.Peek() != -1) //check if there are still characters left
                {
                    message += (char)reader.Read(); //read character from stream and cast into char
                }

                Request request = Request.GetRequest(message);      //get request
                Response response = Response.GetResponse(request, requestHandler);  //get fitting response for request

                StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true }; //writes characters to stream (flushes its buffer to stream)
                writer.Write(response.ResponseString()); //write string to stream

                client.Close();

            } 
            catch (Exception e)
            {
                Console.WriteLine("\nSERVER error: " + e.Message);
                client.Close();
            }

        }
    }

    
}
