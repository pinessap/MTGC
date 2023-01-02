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
        private ConcurrentDictionary<string, User.Trading> tradingDeals;

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
            tradingDeals = new ConcurrentDictionary<string, Trading>();
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
            //Console.WriteLine("getDeck: " + users[username].Deck.cards.Count());
            if (users[username].Deck.cards.Count() == 0)
            {
                return string.Empty;
            }
            
            if (json == false)
            {
                string userDeck = "";
                foreach (var entry in users[username].Deck.cards)
                {
                    userDeck += entry.Value.Id + ", " + entry.Value.Name + ", " + entry.Value.Damage + "\n";
                }
                return userDeck;
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
                return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            }
            catch (Exception e)
            {
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
                }

                foreach (string cardID in array)
                {
                    if (!users[username].MoveCardToDeck(cardID))
                    {
                        return -2;
                    }
                }
                return 0;
                


            } catch (Exception e) 
            { 
                Console.WriteLine("\nerror: " + e.Message);
                return -2;
            }

            return -2;
        }
        public string GetTradings(string username)
        {
            if (tradingDeals.Count() == 0)
            {
                return string.Empty;
            }
            try
            {
                JArray tradingArray = JArray.FromObject(
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
        public int CreateTradingDeal(string username, string jsonBody)
        {
            try
            {
                JObject jsonObject = JObject.Parse(jsonBody);
                string id = jsonObject.GetValue("Id").Value<string>();
                if (tradingDeals.ContainsKey(id))
                {
                    return -1;
                }
                string cardID = jsonObject.GetValue("CardToTrade").Value<string>();
                if (!users[username].Stack.cards.ContainsKey(cardID)) 
                {
                    Console.WriteLine("stack doesn't contain card");
                    return -2;
                }
                string type = jsonObject.GetValue("Type").Value<string>();
                double damage = jsonObject.GetValue("MinimumDamage").Value<double>();

                Card tradingCard = users[username].Stack.RemoveCard(cardID);
                if (tradingCard == null)
                {
                    Console.WriteLine("removing card from stack didn't work");
                    return -3;
                }

                Trading tmptrade = new Trading(id, tradingCard, type, damage, username);

                if (tradingDeals.TryAdd(tmptrade.Id.ToString(), tmptrade))
                {
                    return 0;
                }
                else
                {
                    Console.WriteLine("adding trade to trading Deals didnt work");
                    users[username].Stack.AddCard(tradingCard);
                    return -3;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return -3;
            }

        }
        public int DeleteTradingDeal(string username, string tradeID)
        {
            try
            {
                if (!tradingDeals.ContainsKey(tradeID))
                {
                    return -2;
                }

                string cardID = tradingDeals[tradeID].CardToTrade.Id.ToString();
                if (tradingDeals[tradeID].Owner != username)
                {
                    Console.WriteLine("stack doesn't contain card");
                    return -1;
                }


                if (!tradingDeals.TryRemove(tradeID, out Trading tmpTrade) || tmpTrade == null)
                {
                    return -3;
                }

                if (users[username].Stack.AddCard(tmpTrade.CardToTrade))
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
        public int TradeCard(string username, string tradeID, string jsonBody)
        {
            string offeredCard = JsonConvert.DeserializeObject<string>(jsonBody);
            try
            {
                if (!tradingDeals.ContainsKey(tradeID))
                {
                    return -2;
                }

                string cardID = tradingDeals[tradeID].CardToTrade.Id.ToString();

                if (tradingDeals[tradeID].Owner == username || users[username].Stack.cards.ContainsKey(cardID) || users[username].Deck.cards.ContainsKey(cardID))
                {
                    Console.WriteLine("can't trade with yourself");
                    return -3;
                }

                if (users[username].Deck.cards.ContainsKey(offeredCard) || !users[username].Stack.cards.ContainsKey(offeredCard))
                {
                    Console.WriteLine("offered card not owned or in deck");
                    return -1;
                }

                Card tmpCard = users[username].Stack.cards[offeredCard];
                Trading tmpTrade = tradingDeals[tradeID];

                bool correctType;
                if (tmpTrade.WantedType.ToLower().Contains("spell"))
                {
                    correctType = tmpCard.Name.ToLower().Contains("spell");
                    Console.WriteLine("spell - cardname: " + tmpCard.Name);
                    Console.WriteLine("correct: " + correctType);
                }
                else
                {
                    correctType = !tmpCard.Name.ToLower().Contains("spell");
                    Console.WriteLine("not spell - cardname: " + tmpCard.Name);
                    Console.WriteLine("correct: " + correctType);
                }

                if (!correctType || tmpCard.Damage < tmpTrade.WantedDamage)
                {
                    Console.WriteLine("name - type" + tmpCard.Name + tmpTrade.WantedType);
                    Console.WriteLine("damage - min damage" + tmpCard.Damage + tmpTrade.WantedDamage);
                    Console.WriteLine("requirements are not met");
                    return -1;
                }

                if (!tradingDeals.TryRemove(tradeID, out Trading outTrade) || outTrade == null)
                {
                    return -3;
                }

                Card cardOffered = users[username].Stack.RemoveCard(offeredCard);
                if (cardOffered == null)
                {
                    tradingDeals.TryAdd(tradeID, tmpTrade);
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

                if (users[username].Stack.AddCard(tmpTrade.CardToTrade) && users[tmpTrade.Owner].Stack.AddCard(cardOffered))
                {
                    return 0;
                }
                tradingDeals.TryAdd(tradeID, tmpTrade);
                Console.WriteLine("adding cards failed\n");
                return -3;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return -3;
            }

        }
        public bool QueueBattle(string username)
        {
            
        }
    }
}
