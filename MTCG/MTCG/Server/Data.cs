using Newtonsoft.Json;
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

namespace MTCG.Server
{
    internal class Data
    {
        private ConcurrentDictionary<string, User.User> users;

        private List<string> CardIDs;
        private ConcurrentBag<Collection> packages;

        private static readonly Data serverData = new Data();
        public static Data ServerData
        {
            get { return serverData; }
        }
        private Data()
        {
            users = new ConcurrentDictionary<string, User.User>();
            CardIDs = new List<string>();
            packages = new ConcurrentBag<Collection>();
        }

        public bool RegisterUser(string jsonBody)
        {
            User.User newUser = JsonConvert.DeserializeObject<User.User>(jsonBody);
            return users.TryAdd(newUser.Username, newUser);
        }

        public string GetToken(string jsonBody)
        {
            User.User user = JsonConvert.DeserializeObject<User.User>(jsonBody);
            if (users.ContainsKey(user.Username) && user.Password == users[user.Username].Password)
            {
                string token = users[user.Username].Token;
                JObject jsonObject = new JObject{{"token", token}};
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                //return token;
            }
            return string.Empty;
        }

        public string GetUserData(string username, string token)
        {
            JObject jsonObject = new JObject { { "Name", users[username].Name }, { "Bio", users[username].Bio }, { "Image", users[username].Image } };
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            //return JsonConvert.SerializeObject(users[username]);
        }

        public bool UpdateUserData(string username, string jsonBody)
        {
            try
            {
                JObject userData = JObject.Parse(jsonBody);
                users[username].Name = userData.GetValue("Name").Value<string>();
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

        public string GetUserStats(string username)
        {
            JObject jsonObject = new JObject { { "Name", users[username].Name }, { "Elo", users[username].Elo }, { "Wins", users[username].Wins }, { "Losses", users[username].Losses } };
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        }

        public string GetScoreboard()
        {
   
            JObject jsonObject = JObject.FromObject(new
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
            return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        }

        public bool AddPackage(string jsonBody)
        {

            try
            {
                string id;
                string name;
                double damage;

                Collection package = new Collection(5);
                JArray array = JArray.Parse(jsonBody);
                foreach (JObject card in array.Children<JObject>())
                {
                    id = card.GetValue("Id").Value<string>();
                    if (CardIDs.Contains(id))
                    {
                        return false;
                    }
                    name = card.GetValue("Name").Value<string>();
                    damage = card.GetValue("Damage").Value<double>();

                    Card tmpCard = Card.CreateCard(name, damage, id);
   
                    if (package.AddCard(tmpCard, true))
                    {
                        CardIDs.Add(id);
                    }
                    else
                    {
                        return false;
                    }
                }
                packages.Add(package);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return true;
            }

        }

        public int BuyPackage(string username)
        {
            if (packages.Count == 0)
            {
                return 0;
            }

            if (users[username].Coins < 5)
            {
                return -1;
            }

            bool tmp = packages.TryTake(out Collection result);
            if (tmp && result != null)
            {
                users[username].Coins -= 5;
                foreach (KeyValuePair<string, Card> card in result.cards)
                {
                    users[username].Stack.AddCard(card.Value);
                }

                return 1;
            }

            return -2;

        }

        public string GetStack(string username)
        {
            try
            {
                JObject jsonObject = JObject.FromObject(new
                {
                    Stack = (from entry in users[username].Stack.cards
                        select new
                        {
                            Id = entry.Value.Id,
                            Name = entry.Value.Name,
                            Damage = entry.Value.Damage,
                        })
                });
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
        }
        public string GetDeck(string username, bool json = true)
        {
            int i = 0;
            Console.WriteLine("getDeck: " + users[username].Deck.cards.Count());
            if (users[username].Deck.cards.Count() == 0)
            {
                Console.WriteLine("getDeck1" );
                return string.Empty;
            }
            try
            {
                JObject jsonObject = JObject.FromObject(new
                {
                    Deck = (from entry in users[username].Deck.cards
                        select new
                        {
                            Id = entry.Value.Id,
                            Name = entry.Value.Name,
                            Damage = entry.Value.Damage,
                        })
                });
                Console.WriteLine("getDeck2");
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            }
            catch (Exception e)
            {
                Console.WriteLine("getDeck3");
                Console.WriteLine("error: " + e.Message);
                return string.Empty;
            }
            return string.Empty;
        }

        public int ConfigureDeck(string username, string jsonBody)
        {
            try
            {
                JArray array = JArray.Parse(jsonBody);

                if (array.Count != 4)
                {
                    return 1;
                }

                foreach (string cardID in array)
                {
                    Console.WriteLine("card ID: " + cardID);
                    if (!(users[username].Stack.cards.ContainsKey(cardID) ||
                          users[username].Deck.cards.ContainsKey(cardID)))
                    {
                        return -1;
                    }
                }

                if (users[username].Deck.cards.Count != 0)
                {
                    List<string> tmpList = new List<string>();
                    foreach (KeyValuePair<string, Card> card in users[username].Deck.cards)
                    {
                        tmpList.Add(card.Key);
                    }

                    foreach (string cardID in tmpList)
                    {
                        if (!users[username].MoveCardToStack(cardID))
                        {
                            return -2;
                        }
                    }

                    foreach (string cardID in array)
                    {
                        if (!users[username].MoveCardToDeck(cardID))
                        {
                            return -2;
                        }
                    }
                    return 0;
                }


            } catch (Exception e) 
            { 
                Console.WriteLine("\nerror: " + e.Message);
                return -2;
            }

            return -2;
        }
    }
}
