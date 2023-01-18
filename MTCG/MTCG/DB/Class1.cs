using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;
using MTCG.User;

namespace MTCG.DB
{
    internal class Class1 : IDatabase
    {
        public ConcurrentDictionary<string, User.User> LoadUsers()
        {
            throw new NotImplementedException();
        }

        public ConcurrentBag<string> LoadCardIDs()
        {
            throw new NotImplementedException();
        }

        public ConcurrentBag<Collection> LoadPackages()
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, Card> LoadCards()
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, Trading> LoadTradings(ConcurrentDictionary<string, Card> cards)
        {
            throw new NotImplementedException();
        }

        public bool CreateUser(User.User user)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(string username, string name, string bio, string image)
        {
            throw new NotImplementedException();
        }

        public bool CreatePackage(Collection package)
        {
            throw new NotImplementedException();
        }

        public bool BuyPackage(string username, Collection package)
        {
            throw new NotImplementedException();
        }

        public bool ChangeCardOwnerPackageID(string username, string packID)
        {
            throw new NotImplementedException();
        }

        public bool RemovePackage(string packID)
        {
            throw new NotImplementedException();
        }

        public bool ChangeCoins(string username)
        {
            throw new NotImplementedException();
        }

        public bool RemoveDeck(string username)
        {
            throw new NotImplementedException();
        }

        public bool AddDeck(string username, string cardID)
        {
            throw new NotImplementedException();
        }

        public bool CreateTrade(string tradeID, string cardID, string type, double damage, string username)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTrade(string tradeID, string cardID)
        {
            throw new NotImplementedException();
        }

        public bool PerformTrade(string tradeID, string cardID1, string cardID2, string username1, string username2)
        {
            throw new NotImplementedException();
        }

        public bool Battle(User.User user1, User.User user2, string log)
        {
            throw new NotImplementedException();
        }
    }
}
