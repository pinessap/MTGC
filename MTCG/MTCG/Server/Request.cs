using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server
{
    public class Request
    {
        public string Method { get; }
        public string Path { get; }
        public string Host { get; }
        public string Body { get; }
        public Dictionary<string, string> Headers { get; }
        public Dictionary<string, string> QueryParameters { get; }

        private Request(string method, string path, string host, Dictionary<string, string> headers, // constructor
            Dictionary<string, string> queryParameters, string body = "")
        {
            Method = method;
            Path = path;
            Host = host;
            Headers = headers;
            QueryParameters = queryParameters;
            Body = body;

            Console.WriteLine("method: " + method);
            Console.WriteLine("path: " + path);
            Console.WriteLine("host: " + host);
            Console.WriteLine("headers: " + headers);
            Console.WriteLine("query: " + queryParameters);
            Console.WriteLine("body: \n" + body);
        }

        public static Request GetRequest(string request)
        {
            if (string.IsNullOrEmpty(request)) //check if Request is empty
            {
                return null;
            }
            
            //Console.WriteLine("\nREQUEST START\n" + request + "\nREQUEST END\n");

            request = request.Replace("\r", string.Empty);

            string[] tokens = request.Split('\n'); //get every new line in tokens

            //first line e.g.: POST /transactions/packages HTTP/1.1
            string method = tokens[0].Split(' ')[0]; 
            string path = tokens[0].Split(' ')[1];

            //----- query parameters -----
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            //e.g. deck?format=plain
            if (path.Contains('?') && path.Split('?').Length == 2)
            {
                string[] pathArr = path.Split('?');
                path = pathArr[0];
                string paramStr = pathArr[1];
                string[] paramArr = paramStr.Split('&'); //if there were more than one param
                foreach (var p in paramArr)
                {
                    string[] keyVal = p.Split('=');
                    if (keyVal.Length == 2)
                    {
                        queryParams.Add(keyVal[0], keyVal[1]);
                    }
                }
            }

            //----- headers -----
            Dictionary<string, string> headers = new Dictionary<string, string>();
            int bodyStartIdx = -1;
            string body = string.Empty;

            for (int i = 1; i < tokens.Length; i++)
            {
                if (string.IsNullOrEmpty(tokens[i]) || tokens[i] == "\r") //empty line between headers and body
                {
                    bodyStartIdx = i;
                    break;
                }

                //get headers
                //e.g.: Authorization: Basic kienboec-mtcgToken
                string key = tokens[i].ToLower().Substring(0, tokens[i].IndexOf(':'));
                string val = tokens[i].Substring(tokens[i].IndexOf(':') + 1).Trim();
                headers.Add(key, val);
               
            }

            if (bodyStartIdx != -1)
            {
                for (int i = bodyStartIdx + 1; i < tokens.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(tokens[i]))
                    {
                        body += tokens[i] + Environment.NewLine; //get body in string
                    }
                }
            }

            //e.g.: Host: localhost:10001
            if (headers.ContainsKey("host"))
            {
                return new Request(method, path, headers["host"], headers, queryParams, body);
            }

            return null;
        }
    }
}
