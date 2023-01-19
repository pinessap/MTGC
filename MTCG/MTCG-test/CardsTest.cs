using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using MTCG.Cards;
using MTCG.Cards.Enums;
using MTCG.DB;
using MTCG.Server;

namespace MTCG_test
{
    public class CardsTest
    {
        private ServerMethods server;
        private Mock<IDatabase> mockDB;

        private string json;
        private string username;

        [SetUp]
        public void Setup()
        {
            mockDB = new Mock<IDatabase>();
            server = new ServerMethods(true, mockDB.Object);

            json = "[" + Environment.NewLine +
                   "{" + Environment.NewLine +
                   "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," + Environment.NewLine +
                   "\"Name\": \"WaterGoblin\"," + Environment.NewLine +
                   "\"Damage\": 55" + Environment.NewLine +
                   "}," + Environment.NewLine +
                   "{" + Environment.NewLine +
                   "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," + Environment.NewLine +
                   "\"Name\": \"Dragon\"," + Environment.NewLine +
                   "\"Damage\": 50" + Environment.NewLine +
                   "}," + Environment.NewLine +
                   "{" + Environment.NewLine +
                   "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," + Environment.NewLine +
                   "\"Name\": \"WaterSpell\"," + Environment.NewLine +
                   "\"Damage\": 45" + Environment.NewLine +
                   "}," + Environment.NewLine +
                   "{" + Environment.NewLine +
                   "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," + Environment.NewLine +
                   "\"Name\": \"Ork\"," + Environment.NewLine +
                   "\"Damage\": 40" + Environment.NewLine +
                   "}," + Environment.NewLine +
                   "{" + Environment.NewLine +
                   "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83c04\"," + Environment.NewLine +
                   "\"Name\": \"FireSpell\"," + Environment.NewLine +
                   "\"Damage\": 35" + Environment.NewLine +
                   "}," + Environment.NewLine +
                   "]";

            server.RegisterUser("{" + Environment.NewLine +
                                "\"Username\": \"John\"," + Environment.NewLine +
                                "\"Password\": \"Adams\"" + Environment.NewLine +
                                "}");

            username = "John";

            server.AddPackage(json);
            server.BuyPackage(username);
        }

        [Test]
        public void CreateCard()
        {
            Card tmpGoblin = Card.CreateCard("WaterGoblin", 55, "845f0dc7-37d0-426e-994e-43fc3ac83d05");
            Assert.AreEqual("WaterGoblin", tmpGoblin.Name);
            Assert.AreEqual(Element.Water, tmpGoblin.Element);
            Assert.AreEqual(55, tmpGoblin.Damage);
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83d05", tmpGoblin.Id.ToString());

            Card tmpSpell = Card.CreateCard("FireSpell", 40, "845f0dc7-37d0-426e-994e-43fc3ac83d06");
            Assert.AreEqual("FireSpell", tmpSpell.Name);
            Assert.AreEqual(Element.Fire, tmpSpell.Element);
            Assert.AreEqual(40, tmpSpell.Damage);
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83d06", tmpSpell.Id.ToString());

            Card normCard = Card.CreateCard("Ork", 40, "845f0dc7-37d0-426e-994e-43fc3ac83d00");
            Assert.AreEqual("Ork", normCard.Name);
            Assert.AreEqual(Element.Normal, normCard.Element);
            Assert.AreEqual(40, normCard.Damage);
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83d00", normCard.Id.ToString());
        }
        [Test, Order(1)]
        public void ShowStack()
        {
            Assert.IsNotEmpty(
                server.GetStack(
                    username)); //can't assert exact output string because sequence of cards is always random 

            Assert.IsEmpty(server.GetStack("Martin")); //should fail because Martin does not exist

            Assert.IsEmpty(server.GetDeck(username));
        }

        [Test, Order(2)]
        public void ConfigureDeck()
        {
            Assert.IsEmpty(server.GetDeck(username)); //no deck configured yet -> should fail

            Assert.AreEqual(1, server.ConfigureDeck(username, "[" + Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c04\"," +
                                                              Environment.NewLine +
                                                              "]")); //1 because deck can only consist of 4 cards

            Assert.AreEqual(-1, server.ConfigureDeck(username, "[" + Environment.NewLine +
                                                               "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                                               Environment.NewLine +
                                                               "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                                               Environment.NewLine +
                                                               "\"845f0dc7-37d0-426e-994e-43fc3ac83c09\"," +
                                                               Environment.NewLine +
                                                               "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                                               Environment.NewLine +
                                                               "]")); //-1 because user doesn't own card with ending ID 09

            mockDB.Setup(x => x.RemoveDeck(It.IsAny<string>())).Returns(true);
            mockDB.Setup(x => x.AddDeck(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Assert.AreEqual(0, server.ConfigureDeck(username, "[" + Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                                              Environment.NewLine +
                                                              "]")); //0 because should work

            Assert.AreEqual(0, server.ConfigureDeck(username, "[" + Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," +
                                                              Environment.NewLine +
                                                              "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                                              Environment.NewLine +
                                                              "]")); //0 because should work not only once and with same cards even

            mockDB.Verify(s => s.RemoveDeck(It.IsAny<string>()), Times.AtLeastOnce);
            mockDB.Verify(s => s.AddDeck(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            Assert.False(server.users[username].Deck.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c04"));

            Assert.True(server.users[username].Deck.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c05"));
            Assert.True(server.users[username].Deck.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c06"));
            Assert.True(server.users[username].Deck.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c07"));
            Assert.True(server.users[username].Deck.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));


            Assert.AreEqual("Ork", server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Name);
            Assert.AreEqual(40, server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Damage);

            Assert.AreEqual("WaterSpell",
                server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Name);
            Assert.AreEqual(45, server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Damage);

            Assert.AreEqual("Dragon", server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Name);
            Assert.AreEqual(50, server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Damage);

            Assert.AreEqual("WaterGoblin",
                server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Name);
            Assert.AreEqual(55, server.users[username].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Damage);


            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c04"));
            
            Assert.False(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c05"));
            Assert.False(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c06"));
            Assert.False(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c07"));
            Assert.False(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));
        }

        [Test, Order(3)]
        public void ShowDeck()
        {
            server.ConfigureDeck(username, "[" + Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                           Environment.NewLine +
                                           "]");

            Assert.IsNotEmpty(
                server.GetDeck(
                    username)); //can't assert exact output string because sequence of cards is always random 
        }
    }
}
