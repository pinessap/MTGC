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
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string Host { get; private set; }
        public string Body { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public Dictionary<string, string> QueryParameters { get; private set; }

        private Request(string method, string path, string host, Dictionary<string, string> headers,
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
            Console.WriteLine("body: " + body);
        }

        public static Request GetRequest(string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                return null;
            }

            request = request.Replace("\r", string.Empty);
            string[] tokens = request.Split('\n');

            string method = tokens[0].Split(' ')[0];
            string path = tokens[0].Split(' ')[1];
            //string host = tokens[3];

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            if (path.Contains('?') && path.Split('?').Length == 2)
            {
                string[] pathArr = path.Split('?');
                path = pathArr[0];
                string paramStr = pathArr[1];
                string[] paramArr = paramStr.Split('&');
                foreach (var p in paramArr)
                {
                    string[] keyVal = p.Split('=');
                    if (keyVal.Length == 2)
                    {
                        queryParams.Add(keyVal[0], keyVal[1]);
                    }
                }
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            int bodyStartIdx = -1;
            string body = string.Empty;

            for (int i = 1; i < tokens.Length; i++)
            {
                if (string.IsNullOrEmpty(tokens[i]) || tokens[i] == "\r")
                {
                    bodyStartIdx = i;
                    break;
                }
                
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
                        body += tokens[i] + Environment.NewLine;
                    }
                }
            }

            if (headers.ContainsKey("host"))
            {
                return new Request(method, path, headers["host"], headers, queryParams, body);
            }

            return null;

        }


    }
}
