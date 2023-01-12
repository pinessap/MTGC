﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MTCG.Cards;
using Newtonsoft.Json.Linq;
using MTCG.User;
using System.Diagnostics;
using System.Xml.Linq;

namespace MTCG.Server
{
    internal class ServerMethods
    {
        private ConcurrentDictionary<string, User.User> users;                      // Dictionary of all registered Users 
                                                                          
        //private List<string> CardIDs;
        private ConcurrentBag<string> CardIDs; // List of all Card IDs
        private ConcurrentBag<Collection> packages;                                 // Bag of all packages (package = Collection of Cards)
        private ConcurrentDictionary<string, User.Trading> tradingDeals;            // Dictionary of all Trading Deals
        private ConcurrentQueue<string> battleQueue;                                // Queue of Usernames for Battle

        private static readonly ServerMethods serverData = new ServerMethods();     // Instance of ServerMethods
        public static ServerMethods ServerData                                      // Getter of ServerMethods Instance
        {
            get { return serverData; }
        }
        private ServerMethods()                                                     // Constructor
        {
            users = new ConcurrentDictionary<string, User.User>();
            //CardIDs = new List<string>();
            CardIDs = new ConcurrentBag<string>();
            packages = new ConcurrentBag<Collection>();
            tradingDeals = new ConcurrentDictionary<string, Trading>();
            battleQueue = new ConcurrentQueue<string>();
        }

        public bool RegisterUser(string jsonBody) // register new User 
        {
            User.User newUser = JsonConvert.DeserializeObject<User.User>(jsonBody); //convert json body into User
            return users.TryAdd(newUser.Username, newUser); //add user to users dictionary
        }
        public string GetToken(string jsonBody) // get Token if Username and Password match
        {
            User.User user = JsonConvert.DeserializeObject<User.User>(jsonBody);
            if (users.ContainsKey(user.Username) && user.Password == users[user.Username].Password) //check if user exists and if password is correct
            {
                string token = users[user.Username].Token;
                JObject jsonObject = new JObject{{"token", token}}; //convert into json
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented); //convert into json string
                //return token;
            }
            return string.Empty;
        }
        public string GetUserData(string username, string token) // get own User Data 
        {
            JObject jsonObject = new JObject { { "Name", users[username].Name }, { "Bio", users[username].Bio }, { "Image", users[username].Image } };
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        }
        public bool UpdateUserData(string username, string jsonBody) // update own User Data 
        {
            try
            {
                JObject userData = JObject.Parse(jsonBody);               //parse json body to json object
                users[username].Name = userData.GetValue("Name").Value<string>(); //set variable to value from json body
                users[username].Bio = userData.GetValue("Bio").Value<string>();
                users[username].Image = userData.GetValue("Image").Value<string>();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
        }
        public string GetUserStats(string username) // get user Stats 
        {
            JObject jsonObject = new JObject
            {
                { "Name", users[username].Name }, 
                { "Elo", users[username].Elo }, 
                { "Wins", users[username].Wins }, 
                { "Losses", users[username].Losses }
            };
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        }
        public string GetScoreboard() //get Object of Scoreboard (change to Array?) 
        {
            try
            {
                /*JObject jsonObject = JObject.FromObject(new
                {
                    Scoreboard = (from entry in users
                        orderby entry.Value.Elo ascending
                            select new
                            {
                                Name = entry.Value.Username,
                                Elo = entry.Value.Elo,
                                Wins = entry.Value.Wins,
                                Losses = entry.Value.Losses
                            })
                });
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);*/

                JArray scoresArray = JArray.FromObject( //create json array from users dictionary and order by elo
                    from entry in users
                    orderby entry.Value.Elo ascending
                    select new
                    {
                        Name = entry.Value.Username,
                        Elo = entry.Value.Elo,
                        Wins = entry.Value.Wins,
                        Losses = entry.Value.Losses
                    });
                return JsonConvert.SerializeObject(scoresArray, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
        }
        public bool AddPackage(string jsonBody) // Create Packages 
        {
            try
            {
                string id;
                string name;
                double damage;

                Collection package = new Collection(5); //package = collection with 5 elements 

                JArray array = JArray.Parse(jsonBody); //convert json body into json array
                lock (CardIDs)
                {
                    foreach (JObject card in array.Children<JObject>()) //array consists of objects (= cards)
                    {
                        id = card.GetValue("Id").Value<string>(); //retrieve id as string
                        if (CardIDs.Contains(id))       //id has to be unique -> check if already in bag of card id's
                        {
                            return false;
                        }
                        name = card.GetValue("Name").Value<string>();
                        damage = card.GetValue("Damage").Value<double>();

                        Card tmpCard = Card.CreateCard(name, damage, id); //create card
       
                        if (package.AddCard(tmpCard, true)) //add card to package
                        {
                            CardIDs.Add(id); //add card id to bag
                        }
                        else
                        {
                            return false;
                        }
                    }
                    packages.Add(package); //add package to packages bag
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return true;
            }
        }
        public int BuyPackage(string username) //buy a Package with Coins 
        {
            if (packages.Count == 0) //check if packages available to buy
            {
                return 0;
            }

            if (users[username].Coins < 5) //check if user has enough coins
            {
                return -1;
            }

            bool tmp = packages.TryTake(out Collection result); //take package out of packages bag
            if (tmp && result != null)
            {
                users[username].Coins -= 5;
                foreach (KeyValuePair<string, Card> card in result.cards)
                {
                    users[username].Stack.AddCard(card.Value); //add cards of package to user's stack
                }
                return 1;
            }
            return -2;
        }
        public string GetStack(string username) // get Object of Stack (change to Array?) 
        {
            try
            {
                /*JObject jsonObject = JObject.FromObject(new
                {
                    Stack = (from entry in users[username].Stack.cards
                        select new
                        {
                            Id = entry.Value.Id,
                            Name = entry.Value.Name,
                            Damage = entry.Value.Damage,
                        })
                });
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);*/

                JArray stackArray = JArray.FromObject( //create json array from users stack (= collection) 
                    from entry in users[username].Stack.cards
                    select new
                    {
                        Id = entry.Value.Id,
                        Name = entry.Value.Name,
                        Damage = entry.Value.Damage,
                    });
                return JsonConvert.SerializeObject(stackArray, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
        }
        public string GetDeck(string username, bool json = true) // get Object of Deck (change to Array?) 
        {
            int i = 0;
            if (users[username].Deck.cards.Count() == 0)
            {
                return string.Empty;
            }
            
            if (json == false) //check if format is plain
            {
                string userDeck = "";
                foreach (var entry in users[username].Deck.cards)
                {
                    userDeck += entry.Value.Id + ", " + entry.Value.Name + ", " + entry.Value.Damage + "\n"; //add card values to string
                }
                return userDeck;
            }

            try
            {
                /*JObject jsonObject = JObject.FromObject(new
                {
                    Deck = (from entry in users[username].Deck.cards
                        select new
                        {
                            Id = entry.Value.Id,
                            Name = entry.Value.Name,
                            Damage = entry.Value.Damage,
                        })
                });
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);*/

                JArray deckArray = JArray.FromObject( //create json array from users stack (= collection) 
                    from entry in users[username].Deck.cards
                    select new
                    {
                        Id = entry.Value.Id,
                        Name = entry.Value.Name,
                        Damage = entry.Value.Damage,
                    });
                return JsonConvert.SerializeObject(deckArray, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
            return string.Empty;
        }
        public int ConfigureDeck(string username, string jsonBody) // put together a Deck 
        {
            try
            {
                JArray array = JArray.Parse(jsonBody);

                if (array.Count != 4) //deck has to consist of 4 cards (check if 4 cards are specified in json body)
                {
                    return 1;
                }

                foreach (string cardID in array)
                {
                    if (!(users[username].Stack.cards.ContainsKey(cardID) || //check whether user owns card
                          users[username].Deck.cards.ContainsKey(cardID)))
                    {
                        return -1;
                    }
                }

                if (users[username].Deck.cards.Count != 0)  //check if deck has already cards in it
                {
                    List<string> tmpList = new List<string>(); 
                    foreach (KeyValuePair<string, Card> card in users[username].Deck.cards)
                    {
                        tmpList.Add(card.Key); //add id's of cards from deck to temporary list
                    }

                    foreach (string cardID in tmpList)
                    {
                        if (!users[username].MoveCardToStack(cardID)) //move card from deck to stack
                        {
                            return -2;
                        }
                    }
                }

                foreach (string cardID in array)
                {
                    if (!users[username].MoveCardToDeck(cardID)) //move card from stack to deck
                    {
                        return -2;
                    }
                }
                return 0;

            } 
            catch (Exception e) 
            { 
                Console.WriteLine("\nerror: " + e.Message);
                return -2;
            }

            return -2;
        }
        public string GetTradings(string username) // get Array of all Trading Deals 
        {
            if (tradingDeals.Count() == 0) //check if there are any trading deals
            {
                return string.Empty;
            }

            try
            {
                JArray tradingArray = JArray.FromObject( //make json array of all trading deals
                    from entry in tradingDeals 
                        select new
                        {
                            Id = entry.Value.Id,
                            CardToTrade = entry.Value.CardToTrade.Id,
                            Type = entry.Value.WantedType,
                            MinimumDamage = entry.Value.WantedDamage
                        });
                return JsonConvert.SerializeObject(tradingArray, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
        }
        public int CreateTradingDeal(string username, string jsonBody) // create Trading Deal 
        {
            try
            {
                JObject jsonObject = JObject.Parse(jsonBody);

                string id = jsonObject.GetValue("Id").Value<string>(); //get id of trading deal

                if (tradingDeals.ContainsKey(id)) //check if id already exists (id has to be unique)
                {
                    return -1;
                }

                string cardID = jsonObject.GetValue("CardToTrade").Value<string>(); //get id of card to trade

                if (!users[username].Stack.cards.ContainsKey(cardID))  //check if user owns the card in stack
                {
                    Console.WriteLine("stack doesn't contain card");
                    return -2;
                }

                string type = jsonObject.GetValue("Type").Value<string>();              //get wanted type
                double damage = jsonObject.GetValue("MinimumDamage").Value<double>();   //get wanted min damage

                Card tradingCard = users[username].Stack.RemoveCard(cardID);            //remove card from stack
                if (tradingCard == null)
                {
                    Console.WriteLine("removing card from stack didn't work");
                    return -3;
                }

                Trading tmptrade = new Trading(id, tradingCard, type, damage, username); //create trading deal

                if (tradingDeals.TryAdd(tmptrade.Id.ToString(), tmptrade))     //add deal to dictionary of trading deals
                {
                    return 0;
                }
                else
                {
                    Console.WriteLine("adding trade to trading Deals didnt work");
                    users[username].Stack.AddCard(tradingCard);         //if adding deal didn't work -> add card back to stack
                    return -3;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return -3;
            }
        }
        public int DeleteTradingDeal(string username, string tradeID) // delete Trading Deal 
        {
            try
            {
                if (!tradingDeals.ContainsKey(tradeID)) //check if trading deal with that id exists in dictionary of deals
                {
                    return -2;
                }

                //string cardID = tradingDeals[tradeID].CardToTrade.Id.ToString(); 

                if (tradingDeals[tradeID].Owner != username) //check if user is owner of trading deal (and therefore card to trade)
                {
                    Console.WriteLine("stack doesn't contain card");
                    return -1;
                }

                if (!tradingDeals.TryRemove(tradeID, out Trading tmpTrade) || tmpTrade == null) //remove trading deal from dictionary of deals
                {
                    return -3;
                }

                if (users[username].Stack.AddCard(tmpTrade.CardToTrade)) //add card to stack
                {
                    return 0;
                }

                return -3;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return -3;
            }
        }
        public int TradeCard(string username, string tradeID, string jsonBody) // make a Trade 
        {
            try
            {
                string offeredCard = JsonConvert.DeserializeObject<string>(jsonBody);

                if (!tradingDeals.ContainsKey(tradeID)) //check if deal with id exists in dictionary of deals
                {
                    return -2;
                }

                string cardID = tradingDeals[tradeID].CardToTrade.Id.ToString(); //get id of card to trade from deal

                if (tradingDeals[tradeID].Owner == username ||              //check if user tries to trade with themselves
                    users[username].Stack.cards.ContainsKey(cardID) || 
                    users[username].Deck.cards.ContainsKey(cardID)) 
                {
                    Console.WriteLine("can't trade with yourself");
                    return -3;
                }

                if (users[username].Deck.cards.ContainsKey(offeredCard) ||  //check if offered card is in stack 
                    !users[username].Stack.cards.ContainsKey(offeredCard))
                {
                    Console.WriteLine("offered card not owned or in deck");
                    return -1;
                }

                Card tmpCard = users[username].Stack.cards[offeredCard];
                Trading tmpTrade = tradingDeals[tradeID];

                bool correctType;
                if (tmpTrade.WantedType.ToLower().Contains("spell")) //check if wanted card is spell
                {
                    correctType = tmpCard.Name.ToLower().Contains("spell"); //check if offered card is spell
                    Console.WriteLine("spell - cardname: " + tmpCard.Name);
                    Console.WriteLine("correct: " + correctType);
                }
                else //if wanted card is not spell (but monster)
                {
                    correctType = !tmpCard.Name.ToLower().Contains("spell"); //check if offered card is not spell
                    Console.WriteLine("not spell - cardname: " + tmpCard.Name);
                    Console.WriteLine("correct: " + correctType);
                }

                if (!correctType || tmpCard.Damage < tmpTrade.WantedDamage) //check if offered card meets requirments
                {
                    Console.WriteLine("name - type" + tmpCard.Name + tmpTrade.WantedType);
                    Console.WriteLine("damage - min damage" + tmpCard.Damage + tmpTrade.WantedDamage);
                    Console.WriteLine("requirements are not met");
                    return -1;
                }

                if (!tradingDeals.TryRemove(tradeID, out Trading outTrade) || outTrade == null) //remove deal from trading deals dictionary
                {
                    return -3;
                }

                Card cardOffered = users[username].Stack.RemoveCard(offeredCard);   //remove offered card from stack
                if (cardOffered == null)
                {
                    tradingDeals.TryAdd(tradeID, tmpTrade); //if failed add deal back to trading deals
                    return -3;
                }

                /*Console.WriteLine("\nCARDID: \n" + cardID);
                Card cardTradeOwner = users[tmpTrade.Owner].Stack.RemoveCard(cardID);
                if (cardTradeOwner == null)
                {
                    Console.WriteLine("\n third test\n");
                    tradingDeals.TryAdd(tradeID, tmpTrade);
                    return -3;
                }*/

                if (users[username].Stack.AddCard(tmpTrade.CardToTrade) && users[tmpTrade.Owner].Stack.AddCard(cardOffered)) //add cards to respective new owner's stack
                {
                    return 0;
                }

                tradingDeals.TryAdd(tradeID, tmpTrade); //if failed add deal back to trading deals
                Console.WriteLine("adding cards failed\n");

                return -3;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return -3;
            }
        }
        private User.User getUser(string username) // get User from Username 
        {
            User.User user = users[username]; //get user from users dictionary
            return user;
        }
        public string QueueBattle(string username) // queue for Battle (and start Battle if there is an Opponent) 
        {
            string log;
            battleQueue.Enqueue(username); //add username to queue

            Console.WriteLine("queue count: " + battleQueue.Count());

            if (battleQueue.Count() == 1)   //if user only person in queue
            {
                for (int i = 0; i <= 20; i++)   //wait for 20 seconds
                {
                    if (i % 4 == 0) //for every fourth round
                    {
                        Console.WriteLine("Waiting for an Opponent..."); 
                        if (battleQueue.Count() >= 2)   //break if an opponent joined queue
                        {
                            break;
                        }
                    } 
                    Thread.Sleep(1000); //sleep for a second
                }
            }

            lock (battleQueue)
            {
                if (battleQueue.Count() >= 2) //if queue has at least two usernames
                {
                    Console.WriteLine("An Opponent was found!");
                    if (battleQueue.TryDequeue(out string uname1) && battleQueue.TryDequeue(out string uname2)) //dequeue first 2 users from queue 
                    {

                        getUser(uname1).latestBattleLog = string.Empty;
                        getUser(uname2).latestBattleLog = string.Empty;
                        Battle battle = new Battle(getUser(uname1),getUser(uname2)); //initialize battle with the 2 users
                        log = battle.StartBattle(); //start battle
                        getUser(uname1).latestBattleLog = log;
                        getUser(uname2).latestBattleLog = log;
                        return log;
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong!");
                    }
                    
                }
            }
            Thread.Sleep(1000); //wait a second, so other User also gets log (otherwise log is empty)
            return getUser(username).latestBattleLog;
        }
    }
}