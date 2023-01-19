using Moq;
using MTCG.DB;
using MTCG.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;
using MTCG.User;
using NUnit.Framework.Internal;
using System.Numerics;

namespace MTCG_test
{
    public class GameTest
    {
        private ServerMethods server;
        private Mock<IDatabase> mockDB;
        private Battle battle;
        private Mock<Battle> mockBattle; 
        private string jsonPckg1;
        private string jsonPckg2;
        private User John;
        private User Thomas;


        [SetUp]
        public void Setup()
        {
            mockDB = new Mock<IDatabase>();
            server = new ServerMethods(true, mockDB.Object);

            jsonPckg1 = "[" + Environment.NewLine +
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

            jsonPckg2 = "[" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83b04\"," + Environment.NewLine +
                        "\"Name\": \"WaterGoblin\"," + Environment.NewLine +
                        "\"Damage\": 55" + Environment.NewLine +
                        "}," + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83b03\"," + Environment.NewLine +
                        "\"Name\": \"Dragon\"," + Environment.NewLine +
                        "\"Damage\": 50" + Environment.NewLine +
                        "}," + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83b02\"," + Environment.NewLine +
                        "\"Name\": \"WaterSpell\"," + Environment.NewLine +
                        "\"Damage\": 45" + Environment.NewLine +
                        "}," + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83b01\"," + Environment.NewLine +
                        "\"Name\": \"Ork\"," + Environment.NewLine +
                        "\"Damage\": 40" + Environment.NewLine +
                        "}," + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "\"Id\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," + Environment.NewLine +
                        "\"Name\": \"FireSpell\"," + Environment.NewLine +
                        "\"Damage\": 35" + Environment.NewLine +
                        "}," + Environment.NewLine +
                        "]";

            server.RegisterUser("{" + Environment.NewLine +
                                "\"Username\": \"John\"," + Environment.NewLine +
                                "\"Password\": \"Adams\"" + Environment.NewLine +
                                "}");

            server.RegisterUser("{" + Environment.NewLine +
                                "\"Username\": \"Thomas\"," + Environment.NewLine +
                                "\"Password\": \"Jefferson\"" + Environment.NewLine +
                                "}");

            server.AddPackage(jsonPckg1);
            server.AddPackage(jsonPckg2);

            server.BuyPackage("John");
            server.BuyPackage("Thomas");

            server.ConfigureDeck("Thomas", "[" + Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c07\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"," +
                                           Environment.NewLine +
                                           "\"845f0dc7-37d0-426e-994e-43fc3ac83c05\"," +
                                           Environment.NewLine +
                                           "]");

            server.ConfigureDeck("John", "[" + Environment.NewLine +
                                         "\"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                         Environment.NewLine +
                                         "\"845f0dc7-37d0-426e-994e-43fc3ac83b01\"," +
                                         Environment.NewLine +
                                         "\"845f0dc7-37d0-426e-994e-43fc3ac83b02\"," +
                                         Environment.NewLine +
                                         "\"845f0dc7-37d0-426e-994e-43fc3ac83b03\"," +
                                         Environment.NewLine +
                                         "]");
            John = server.users["John"];
            Thomas = server.users["Thomas"];

            battle = new Battle(John, Thomas);
            mockBattle = new Mock<Battle>(John, Thomas);
        }

        [Test, Order(1)]
        public void GetStats()
        {
            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 0,\r\n  \"Losses\": 0\r\n}",
                server.GetUserStats("John"));
            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 0,\r\n  \"Losses\": 0\r\n}",
                server.GetUserStats("Thomas"));
            Assert.IsEmpty(server.GetUserStats("Martin"));
        }

        [Test, Order(2)]
        public void GetScoreboard()
        {
            server.users["John"].Elo += 3;
            Assert.AreEqual(
                "[\r\n  {\r\n    \"Name\": \"Thomas\",\r\n    \"Elo\": 100,\r\n    \"Wins\": 0,\r\n    \"Losses\": 0\r\n  }," +
                "\r\n  {\r\n    \"Name\": \"John\",\r\n    \"Elo\": 103,\r\n    \"Wins\": 0,\r\n    \"Losses\": 0\r\n  }\r\n]",
                server.GetScoreboard());
        }

        [Test, Order(3)]
        public void CheckUserFunctions()
        {


            Assert.False(John.IsDeckEmpty());
            Assert.False(Thomas.IsDeckEmpty());

            Assert.AreEqual(0, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(0, John.NumGames);
            Assert.AreEqual(100, John.Elo);
            John.Win();
            Assert.AreEqual(1, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(1, John.NumGames);
            Assert.AreEqual(103, John.Elo);

            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(0, Thomas.Losses);
            Assert.AreEqual(0, Thomas.NumGames);
            Assert.AreEqual(100, Thomas.Elo);
            Thomas.Loss();
            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(1, Thomas.Losses);
            Assert.AreEqual(1, Thomas.NumGames);
            Assert.AreEqual(95, Thomas.Elo);

            John.Draw();
            Assert.AreEqual(1, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(2, John.NumGames);
            Assert.AreEqual(103, John.Elo);

            Thomas.Draw();
            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(1, Thomas.Losses);
            Assert.AreEqual(2, Thomas.NumGames);
            Assert.AreEqual(95, Thomas.Elo);
        }

        [Test, Order(3)]
        public void CheckPureMonster()
        {
            Card moCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b01"];
            Card moCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"];
            Card speCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b00"];
            Card speCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"];

            Assert.True(battle.CheckPureMonsterFight(moCard1, moCard2));
            Assert.True(battle.CheckPureMonsterFight(moCard2, moCard1));
            Assert.False(battle.CheckPureMonsterFight(moCard1, speCard2));
            Assert.False(battle.CheckPureMonsterFight(moCard2, speCard1));
            Assert.False(battle.CheckPureMonsterFight(speCard1, speCard2));
            Assert.False(battle.CheckPureMonsterFight(speCard2, speCard1));
        }

        [Test, Order(4)]
        public void CheckSpecialities()
        {
            Assert.AreEqual(2, battle.CheckSpecialties("WaterGoblin", "Dragon", 0));
            Assert.AreEqual(0, battle.CheckSpecialties("WaterGoblin", "Dragon", 1));
            Assert.AreEqual(1, battle.CheckSpecialties("Dragon", "WaterGoblin", 0));
            Assert.AreEqual(1, battle.CheckSpecialties("Dragon", "WaterGoblin", 1));

            Assert.AreEqual(1, battle.CheckSpecialties("Wizard", "Ork", 0));
            Assert.AreEqual(0, battle.CheckSpecialties("Wizard", "Ork", 2));
            Assert.AreEqual(2, battle.CheckSpecialties("Ork", "Wizard", 0));
            Assert.AreEqual(2, battle.CheckSpecialties("Ork", "Wizard", 2));

            Assert.AreEqual(2, battle.CheckSpecialties("Knight", "WaterSpell", 0));
            Assert.AreEqual(0, battle.CheckSpecialties("Knight", "WaterSpell", 1));
            Assert.AreEqual(1, battle.CheckSpecialties("WaterSpell", "Knight", 0));
            Assert.AreEqual(1, battle.CheckSpecialties("WaterSpell", "Knight", 1));

            Assert.AreEqual(1, battle.CheckSpecialties("Kraken", "WaterSpell", 0));
            Assert.AreEqual(0, battle.CheckSpecialties("Kraken", "WaterSpell", 2));
            Assert.AreEqual(2, battle.CheckSpecialties("FireSpell", "Kraken", 0));
            Assert.AreEqual(2, battle.CheckSpecialties("FireSpell", "Kraken", 2));

            Assert.AreEqual(1, battle.CheckSpecialties("FireElf", "Dragon", 0));
            Assert.AreEqual(0, battle.CheckSpecialties("FireElf", "Dragon", 2));
            Assert.AreEqual(2, battle.CheckSpecialties("Dragon", "FireElf", 0));
            Assert.AreEqual(2, battle.CheckSpecialties("Dragon", "FireElf", 2));
        }

        [Test, Order(5)]
        public void CheckCardsBattle()
        {
            Card moCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b01"]; //Ork 40
            Card moCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"]; //WaterGoblin 55
            Card speCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b00"]; //FireSpell 35
            Card speCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"]; //WaterSpell 45

            Card ork2Card = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"]; //Ork 40

            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 0));
            Assert.AreEqual(1, battle.CardsBattle(moCard2, moCard1, 0));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, moCard2, 1)); //ork 40*2 > WaterGoblin 55
            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 2));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0)); //normal > water
            Assert.AreEqual(2, battle.CardsBattle(speCard2, moCard1, 0));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 1));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 2)); //80 > 90/2
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0)); //WaterGoblin 55 vs FireSpell 35
            Assert.AreEqual(2, battle.CardsBattle(speCard1, moCard2, 0));
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 2)); //110 > 70/2
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 1));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0));
            Assert.AreEqual(1, battle.CardsBattle(speCard2, speCard1, 0));
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 1));
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 2));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            //check if boost actually works
            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 0));
            Assert.AreEqual(0, battle.CardsBattle(ork2Card, moCard1, 0));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, ork2Card, 1)); //40*2 > 40
            Assert.AreEqual(2, battle.CardsBattle(moCard1, ork2Card, 2));

            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 1)); //boost already used -> doesnt work
        }


        [Test, Order(6)]
        public void CheckBattle()
        {

            Assert.AreEqual(0, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(0, John.NumGames);
            Assert.AreEqual(100, John.Elo);

            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(0, Thomas.Losses);
            Assert.AreEqual(0, Thomas.NumGames);
            Assert.AreEqual(100, Thomas.Elo);

            Assert.IsNotEmpty(mockBattle.Object.StartBattle());

            if (John.Wins == 1)
            {
                Console.WriteLine("John won");
                Assert.AreEqual(1, Thomas.Losses);
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
                Assert.AreEqual(103, John.Elo);
                Assert.AreEqual(95, Thomas.Elo);
            } 
            else if (John.Losses == 1)
            {
                Console.WriteLine("Thomas won");
                Assert.AreEqual(1, Thomas.Wins);
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
                Assert.AreEqual(103, Thomas.Elo);
                Assert.AreEqual(95, John.Elo);
            }
            else
            {
                Console.WriteLine("Game drew");
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
            }

            Assert.AreEqual(4, John.Deck.cards.Count);
            Assert.AreEqual(4, Thomas.Deck.cards.Count);

            Assert.True(Thomas.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83c08"));
            Assert.True(Thomas.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83c07"));
            Assert.True(Thomas.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83c06"));
            Assert.True(Thomas.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83c05"));

            Assert.True(John.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83b03"));
            Assert.True(John.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83b02"));
            Assert.True(John.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83b01"));
            Assert.True(John.Deck.cards.Keys.Contains("845f0dc7-37d0-426e-994e-43fc3ac83b00"));

        }
    }
}
