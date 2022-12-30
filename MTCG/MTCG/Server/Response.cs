using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return new Response("400 Bad Request", "application/json", "{\"msg\": \"Error bad request\"}\n");
            }

            return new Response("400 Bad Request", "text/plain", "Error bad request\n");
        }

        private static Response UnauthorizedResponse(bool asJson = false)
        {
            if (asJson)
            {
                return new Response("401 Unauthorized", "application/json",
                    "{\"msg\": \"Error Unauthorized\"}\n");
            }

            return new Response("401 Unauthorized", "text/plain", "Error Unauthorized\n");
        }

        private static Response PageNotFoundResponse(bool asJson = false)
        {
            if (asJson)
            {
                return new Response("404 Not Found", "application/json",
                    "{\"msg\": \"Error page not found\"}\n");
            }

            return new Response("404 Not Found", "text/plain", "Error page not found\n");
        }

        private static Response MethodNotAllowedResponse(bool asJson = false)
        {
            if (asJson)
            {
                return new Response("405 Method Not Allowed", "application/json",
                    "{\"msg\": \"Error method not allowed\"}\n");
            }

            return new Response("405 Method Not Allowed", "text/plain", "Error method not allowed\n");
        }

        public static Response GetResponse(Request request)
        {
            if (request == null)
            {
                return NullResponse();
            }

            return null;
        }
    }
}
