using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using MTCG.DB;
using Newtonsoft.Json;

namespace MTCG.Server
{
    public class Response
    {
        public string Body { get; private set; }
        public string Status { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }

        private Response(string status, string contentType, string body) // Constructor (set status, body, headers of response) 
        {
            Status = status;
            Body = body;
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", contentType },
                { "Content-Length", Encoding.UTF8.GetBytes(Body).Length.ToString() }
            };
        }

        private static Response NullResponse() // for a request == null 
        { 
            return new Response("400 Bad Request", "application/json", 
                "{\"msg\": \"error: bad request\"}\n");
        }

        private static Response MethodNotAllowedResponse() // if Method != (get, post, put, delete) 
        { 
            return new Response("405 Method Not Allowed", "application/json", 
                "{\"msg\": \"error: method not allowed\"}\n");
        }

        private static Response AccessTokenInvalid() // if token missing or invalid 
        {
            return new Response("401 Unauthorized", "application/json",
                "{\"msg\": \"error: Access token is missing or invalid\"}\n");
        }

        private static Response OtherError() // if some other (not specifically defined) error occured 
        {
            return new Response("409 Conflict", "application/json",  
                "{\"msg\": \"error: another error occured\"}\n");
        }

        
        //-------------------------------------------------------- RESPONSE HANDLER ----------------------------------------------------------------------------------------     
        public static Response GetResponse(Request request) 
        {
            if (request == null) //check first if request null
            {
                return NullResponse();
            }

            ServerMethods serverData = new ServerMethods(new Database());

            switch (request.Method)
            {
                //----------------------- GET METHOD --------------------------------------------------------------------------------------------   
                case "GET":
                    //-------------------------------------- retrieves user data for given username -----------------------------------
                    if (request.Path.Contains("/users/"))
                    {
                        string username = request.Path.Substring(7); //get username from path
                        if (!string.IsNullOrEmpty(username))
                        {
                           // Console.WriteLine("username retrieved: " + username);
                            if (!string.IsNullOrEmpty(GetToken(request.Headers)) && CheckToken(GetToken(request.Headers),username)) //check if token matches or is admin
                            {
                                string body = serverData.GetUserData(username, GetToken(request.Headers)); //get user data
                                if (body != null)
                                {
                                    return new Response("200 OK", "application/json", body + "\n");
                                }
                                else
                                {
                                    return new Response("404 Not Found", "application/json",
                                        "{\"msg\": \"error: User not found\"}\n");
                                }
                            }
                            else
                            {
                                return AccessTokenInvalid();
                            }
                        }
                        else
                        {
                            return NullResponse();
                        }
                    }
                    //-------------------------------------- retrieves stats for individual user --------------------------------------
                    else if (request.Path == "/stats")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body = serverData.GetUserStats(GetUsernameFromToken(GetToken(request.Headers))); // get stats
                            if (body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- retrieves user scoreboard ordered by ELO ---------------------------------
                    else if (request.Path == "/score")
                    {
                        if (!string.IsNullOrEmpty(GetToken(request.Headers)))
                        {
                            string body = serverData.GetScoreboard(); //get scoreboard
                            if (body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- show a user's cards ------------------------------------------------------
                    else if (request.Path == "/cards")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body = serverData.GetStack(GetUsernameFromToken(GetToken(request.Headers))); //get stack
                            if (body != string.Empty && body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                            else
                            {
                                return new Response("200 OK", "application/json", //used 200 instead of 204 because u can't send a body with 204
                                    "{\"msg\": \"error: The request was fine, but the user doesn't have any cards\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- show the user's currently configured deck --------------------------------
                    else if (request.Path == "/deck")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body;
                            if (request.QueryParameters.ContainsKey("format") &&    //check if plain format is requested
                                request.QueryParameters["format"] == "plain")
                            {
                                body = serverData.GetDeck(GetUsernameFromToken(GetToken(request.Headers)), false);  //get Deck as simple string 
                            }
                            else
                            {
                                body = serverData.GetDeck(GetUsernameFromToken(GetToken(request.Headers)), true);  //get Deck in json format
                            }
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
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- retrieves the currently available trading deals --------------------------
                    else if (request.Path == "/tradings")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string body;
                            body = serverData.GetTradings(GetUsernameFromToken(GetToken(request.Headers))); //get trading deals
                            if (body != string.Empty && body != null)
                            {
                                return new Response("200 OK", "application/json", body + "\n");
                            }
                            else
                            {
                                return new Response("200 OK", "application/json",  //used 200 instead of 204 because u can't send a body with 204
                                    "{\"msg\": \"error: The request was fine, but there are no trading deals available\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    break;

                //----------------------- POST METHOD ------------------------------------------------------------------------------------------- 
                case "POST":
                    //-------------------------------------- register a new user ------------------------------------------------------
                    if (request.Path == "/users")
                    {
                        if (serverData.RegisterUser(request.Body)) //register user
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
                        string token = serverData.GetToken(request.Body); //after successful login token will be sent to client
                        if (token != string.Empty)
                        {
                            return new Response("200 OK", "application/json", token + "\n");
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
                            if (GetToken(request.Headers) == "admin-mtcgToken") //check if token is admin's
                            {
                                if (serverData.AddPackage(request.Body)) //add packages
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
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- acquire card package -----------------------------------------------------
                    else if (request.Path == "/transactions/packages")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int result= serverData.BuyPackage(GetUsernameFromToken(GetToken(request.Headers))); //buy packages
                            if (result == 0)
                            {
                                return new Response("404 Not Found", "application/json",
                                    "{\"msg\": \"error: no card package available for buying\"}\n");
                            }
                            else if(result == -1 )
                            {
                                return new Response("403 Forbidden", "application/json",
                                    "{\"msg\": \"error: not enough money for buying a card package\"}\n");
                            } 
                            else if (result == 1)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"a package has been successfully bought\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }

                    }
                    //-------------------------------------- creates new trading deal -------------------------------------------------
                    else if (request.Path == "/tradings")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int response = serverData.CreateTradingDeal(GetUsernameFromToken(GetToken(request.Headers)), request.Body); //create trading deal
                            if (response == 0)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"trading deal successfully created\"}\n");
                            }
                            else if (response == -1)
                            {
                                return new Response("409 Conflict", "application/json",  
                                    "{\"msg\": \"error: a deal with this deal ID already exists\"}\n");
                            } 
                            else if (response == -2)
                            {
                                return new Response("403 Forbidden", "application/json",
                                    "{\"msg\": \"error: \"the deal contains a card that is not owned by the user or locked in the deck\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- carry out a trade for the deal with the provided card --------------------
                    if (request.Path.Contains("/tradings/"))
                    {
                        string tradeID = request.Path.Substring(10); //get trading deal id from path
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int response = serverData.TradeCard(GetUsernameFromToken(GetToken(request.Headers)), tradeID, request.Body); //carry out deal
                            if (response == 0)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"Trading deal successfully executed\"}\n");
                            }
                            else if (response == -1)
                            {
                                return new Response("403 Forbidden", "application/json",
                                    "{\"msg\": \"error: \"The offered card is not owned by the user, " +
                                    "or the requirements are not met (Type, MinimumDamage), " +
                                                "or the offered card is locked in the deck.\"}\n");
                            }
                            else if (response == -2)
                            {
                                return new Response("404 Not Found", "application/json",
                                    "{\"msg\": \"error: \"The provided deal ID was not found.\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    //-------------------------------------- enters the lobby to start a battle ---------------------------------------
                    else if (request.Path == "/battles")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            string response = serverData.QueueBattle(GetUsernameFromToken(GetToken(request.Headers))); //queue for a battle (and start battle if opponent available) 
                            if (!string.IsNullOrEmpty(response))
                            {
                                return new Response("200 OK", "application/json", response + "\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    break;

                //----------------------- PUT METHOD -------------------------------------------------------------------------------------------- 
                case "PUT":
                    //-------------------------------------- updates user data for given username -------------------------------------
                    if (request.Path.Contains("/users/"))
                    {
                        string username = request.Path.Substring(7); //get username from path
                        if (!string.IsNullOrEmpty(username))
                        {
                            if (!string.IsNullOrEmpty(GetToken(request.Headers)) && CheckToken(GetToken(request.Headers), username))
                            {
                                if (serverData.UpdateUserData(username, request.Body)) //update data
                                {
                                    return new Response("200 OK", "application/json",
                                        "{\"msg\": \"User sucessfully updated\"}\n");
                                }
                                else
                                {
                                    return new Response("404 Not Found", "application/json",
                                        "{\"msg\": \"error: User not found\"}\n");
                                }
                            }
                            else
                            {
                                return AccessTokenInvalid();
                            }
                        }
                        else
                        {
                            return NullResponse();
                        }
                    }
                    //-------------------------------------- configures the deck with four provided cards -----------------------------
                    if (request.Path == "/deck")
                    {
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int result = serverData.ConfigureDeck(GetUsernameFromToken(GetToken(request.Headers)), request.Body); //put deck together
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
                            return AccessTokenInvalid();
                        }
                    }
                    break;

                //----------------------- DELETE METHOD -----------------------------------------------------------------------------------------
                case "DELETE":
                    //-------------------------------------- creates new trading deal -------------------------------------------------
                    if (request.Path.Contains("/tradings/"))
                    {
                        string tradeID = request.Path.Substring(10); //get trading deal id from path
                        //Console.WriteLine("DELETE ID: " + tradeID);
                        if (!string.IsNullOrEmpty(GetUsernameFromToken(GetToken(request.Headers))))
                        {
                            int response = serverData.DeleteTradingDeal(GetUsernameFromToken(GetToken(request.Headers)), tradeID); //delete trading deal
                            if (response == 0)
                            {
                                return new Response("200 OK", "application/json",
                                    "{\"msg\": \"Trading deal successfully deleted\"}\n");
                            }
                            else if (response == -1)
                            {
                                return new Response("403 Forbidden", "application/json",
                                    "{\"msg\": \"error: \"The deal contains a card that is not owned by the user\"}\n");
                            }
                            else if (response == -2)
                            {
                                return new Response("404 Not Found", "application/json",
                                    "{\"msg\": \"error: \"The provided deal ID was not found.\"}\n");
                            }
                        }
                        else
                        {
                            return AccessTokenInvalid();
                        }
                    }
                    break;

                //----------------------- DEFAULT ----------------------------------------------------------------------------------------------- 
                default:
                    return MethodNotAllowedResponse();
            }
            return OtherError();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        public string ResponseString() // build the response string (to send to client)
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

        private static string GetToken(Dictionary<string, string> headers) // get token from request headers 
        {
            //Format: --header "Authorization: Basic kienboec-mtcgToken"
            if (headers.ContainsKey("authorization"))
            {
                string[] content = headers["authorization"].Split(' '); //split token from "basic" 
                if (content.Length == 2)
                {
                    //Console.WriteLine("token: " + content[1]); 
                    return content[1]; //return token
                }
            }
            return string.Empty;
        }

        private static bool CheckToken(string token, string username) // check if token and username is matching or if admin 
        {
            string tmp = token.Split('-')[0];
            if (tmp == "admin")                 //check if admin
            {
                return true;
            }

            return tmp == username; //check if token valid (right user)
        }

        private static string GetUsernameFromToken(string token) // retrieve username from token 
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
