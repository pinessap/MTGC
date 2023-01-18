using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;
using MTCG.User;

namespace MTCG.DB
{
    public interface IDatabase
    {
        public ConcurrentDictionary<string, User.User> LoadUsers();
        public ConcurrentBag<string> LoadCardIDs();
        public ConcurrentBag<Collection> LoadPackages();
        public ConcurrentDictionary<string, Cards.Card> LoadCards();
        public ConcurrentDictionary<string, Trading> LoadTradings(ConcurrentDictionary<string, Cards.Card> cards);
        public bool CreateUser(User.User user);
        public bool UpdateUser(string username, string name, string bio, string image);

        public bool CreatePackage(Collection package);

        public bool BuyPackage(string username, Collection package);
        public bool ChangeCardOwnerPackageID(string username, string packID);
        public bool RemovePackage(string packID);
        public bool ChangeCoins(string username);
        public bool RemoveDeck(string username);
        public bool AddDeck(string username, string cardID);
        public bool CreateTrade(string tradeID, string cardID, string type, double damage, string username);
        public bool DeleteTrade(string tradeID, string cardID);
        public bool PerformTrade(string tradeID, string cardID1, string cardID2, string username1, string username2);
        public bool Battle(User.User user1, User.User user2, string log);

    }
}
