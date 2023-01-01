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
                    //-------------------------------------- retrieves user data for given username -----------------------------------
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
                    //-------------------------------------- retrieves stats for individual user --------------------------------------
                    else if (request.Path == "/stats")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body = serverData.GetUserStats(GetUsernameFromToken(GetToken(request.Headers)));
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
                    //-------------------------------------- retrieves user scoreboard ordered by ELO ---------------------------------
                    else if (request.Path == "/score")
                    {
                        if (!string.IsNullOrEmpty(GetToken(request.Headers)))
                        {
                            string body = serverData.GetScoreboard();
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
                    //-------------------------------------- show a user's cards ------------------------------------------------------
                    else if (request.Path == "/cards")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body = serverData.GetStack(GetUsernameFromToken(GetToken(request.Headers)));
                            if (body != string.Empty && body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                            else
                            {
                                return new Response("200 OK", "application/json",//used 200 instead of 204 because u can't send a body with 204
                                    "{\"msg\": \"error: The request was fine, but the user doesn't have any cards\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                        }

                    }
                    //-------------------------------------- show the user's currently configured deck --------------------------------
                    else if (request.Path == "/deck")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body;
                            if (request.QueryParameters.ContainsKey("format") &&
                                request.QueryParameters["format"] == "plain")
                            {
                                body = serverData.GetDeck(GetUsernameFromToken(GetToken(request.Headers)),false);
                            }
                            else
                            {
                                body = serverData.GetDeck(GetUsernameFromToken(GetToken(request.Headers)), true);
                            }
                            //Console.WriteLine("body: " + body);
                            //body = "test";
                            if (body != string.Empty && body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                            else
                            {
                                return new Response("200 OK", "application/json",  //used 200 instead of 204 because u can't send a body with 204
                                    "{\"msg\": \"error: The request was fine, but the deck doesn't have any cards\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                        }

                    }

                    break;
                case "POST":
                    //-------------------------------------- register a new user ------------------------------------------------------
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
                    //-------------------------------------- login with existing user -------------------------------------------------
                    else if (request.Path == "/sessions")
                    {
                        string token = serverData.GetToken(request.Body);
                        if (token != string.Empty)
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
                    //-------------------------------------- create new card packages (as admin) --------------------------------------
                    else if (request.Path == "/packages")
                    {
                        if (!string.IsNullOrEmpty(GetToken(request.Headers)))
                        {
                            if (GetToken(request.Headers) == "admin-mtcgToken")
                            {
                                if (serverData.AddPackage(request.Body))
                                {
                                    return new Response("200 Created", "application/json",
                                        "{\"msg\": \"Package and cards successfully created\"}\n");
                                }
                                else
                                {
                                    return new Response("409 Conflict", "application/json",
                                        "{\"msg\": \"error: At least one card in the packages already exists\"}\n");
                                }
                            }
                            else
                            {
                                return new Response("401 Unauthorized", "application/json",
                                    "{\"msg\": \"error: Provided user is not \"admin\"\"}\n");
                            }

                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                        }
                        
                    }
                    //-------------------------------------- acquire card package -----------------------------------------------------
                    else if (request.Path == "/transactions/packages")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int result= serverData.BuyPackage(GetUsernameFromToken(GetToken(request.Headers)));
                            if (result == 0)
                            {
                                return new Response("404 Not Found", "application/json",
                                    "{\"msg\": \"error: No card package available for buying\"}\n");
                            }
                            else if(result == -1 )
                            {
                                return new Response("403 Forbidden", "application/json",
                                    "{\"msg\": \"error: Not enough money for buying a card package\"}\n");
                            } else if (result == 1)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"A package has been successfully bought\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
                        }

                    }

                    break;
                case "PUT":
                    //-------------------------------------- updates user data for given username -------------------------------------
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
                    if (request.Path.Contains("/deck"))
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int result = serverData.ConfigureDeck(GetUsernameFromToken(GetToken(request.Headers)), request.Body);
                            if (result == 0)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"The deck has been successfully configured\"}\n");
                            }
                            else if (result == 1)
                            {
                                return new Response("400 Bad Request", "application/json",
                                    "{\"msg\": \"error: The provided deck did not include the required amount of cards\"}\n");
                            } else if (result == -1)
                            {
                                return new Response("400 Bad Request", "application/json",
                                    "{\"msg\": \"error: At least one of the provided cards does not belong to the user or is not available.\"}\n");
                            }
                        }
                        else
                        {
                            return new Response("401 Unauthorized", "application/json",
                                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
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
                    Console.WriteLine("token: " + content[1]);
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

        private static string GetUsernameFromToken(string token)
        {
            try
            {
                return token.Replace("-mtcgToken", string.Empty);
            }
            catch (Exception e)
            {
                return string.Empty;
            }

        }


    }
}
