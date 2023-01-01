using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MTCG.Server
{
    public class Response
    {
        public string Body { get; private set; }
        public string Status { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        
        private Response(string status, string contentType, string body)
        {
            Status = status;
            Body = body;
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", contentType },
                { "Content-Length", Encoding.UTF8.GetBytes(Body).Length.ToString() }
            };
        }

        private static Response NullResponse(bool json = false)
        {
            if (json)
            {
                return new Response("400 Bad Request", "application/json", "{\"msg\": \"error: bad request\"}\n");
            }

            return new Response("400 Bad Request", "text/plain", "error: bad request\n");
        }

        private static Response MethodNotAllowedResponse(bool json = false)
        {
            if (json)
            {
                return new Response("405 Method Not Allowed", "application/json",
                    "{\"msg\": \"error: method not allowed\"}\n");
            }

            return new Response("405 Method Not Allowed", "text/plain", "error: method not allowed\n");
        }

        public static Response GetResponse(Request request)
        {
            if (request == null)
            {
                return NullResponse();
            }

            Data serverData = Data.ServerData;

            switch (request.Method)
            {
                case "GET":
                    if (request.Path.Contains("/users/"))
                    {
                        string username = request.Path.Substring(7);
                        if (!string.IsNullOrEmpty(username))
                        {
                           // Console.WriteLine("username retrieved: " + username);
                            if (!string.IsNullOrEmpty(GetToken(request.Headers)) && CheckToken(GetToken(request.Headers),username))
                            {
                                string body = serverData.GetUserData(username, GetToken(request.Headers));
                                if (body != null)
                                {
                                    return new Response("200 OK", "application/json", body + "\n");
                                }
                            }
                            else
                            {
                                return new Response("401 Unauthorized", "application/json",
                                    "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("404 Not Found", "application/json",
                                "{\"msg\": \"error: User not found\"}\n");
                        }
                        


                    }
                    break;
                case "POST":
                    if (request.Path == "/users")
                    {
                        if (serverData.RegisterUser(request.Body))
                        {
                            return new Response("201 Created", "application/json",
                                "{\"msg\": \"User successfully created\"}\n");
                        }
                        else
                        {
                            return new Response("409 Bad Request", "application/json",
                                "{\"msg\": \"error: User with same username already registered\"}\n");
                        }
                    }
                    else if (request.Path == "/sessions")
                    {
                        string token = serverData.GetToken(request.Body);
                        if (token != String.Empty)
                        {
                            return new Response("200 OK", "application/json",
                                token + "\n");
                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Invalid username/password provided\"}\n");
                        }
                    }
                    else if (request.Path == "/packages")
                    {

                    }
                    break;
                case "PUT":
                    if (request.Path.Contains("/users/"))
                    {
                        string username = request.Path.Substring(7);
                        if (!string.IsNullOrEmpty(username))
                        {
                            if (!string.IsNullOrEmpty(GetToken(request.Headers)) && CheckToken(GetToken(request.Headers), username))
                            {
                                //string body = serverData.GetUserData(username, GetToken(request.Headers));
                                if (serverData.UpdateUserData(username, request.Body))
                                {
                                    return new Response("200 OK", "application/json",
                                        "{\"msg\": \"User sucessfully updated\"}\n");
                                }
                            }
                            else
                            {
                                return new Response("401 Unauthorized", "application/json",
                                    "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("404 Not Found", "application/json",
                                "{\"msg\": \"error: User not found\"}\n");
                        }
                    }

                    break;


                default:
                    return MethodNotAllowedResponse();


            }

            return null;
        }

        public string ResponseString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 " + Status);
            foreach (KeyValuePair<string, string> header in this.Headers)
            {
                sb.AppendLine(header.Key + ":" + header.Value);
            }
            sb.AppendLine();
            sb.AppendLine(Body);

            return sb.ToString();
        }

        private static string GetToken(Dictionary<string, string> headers)
        {
            //--header "Authorization: Basic kienboec-mtcgToken"
            if (headers.ContainsKey("authorization"))
            {
                string[] content = headers["authorization"].Split(' ');
                if (content.Length == 2)
                {
                    return content[1];
                }
            }
            return string.Empty;
        }

        private static bool CheckToken(string token, string username)
        {
            string tmp = token.Split('-')[0];
            if (tmp == "admin")
            {
                return true;
            }
            return tmp == username;
        }

        
    }
}
