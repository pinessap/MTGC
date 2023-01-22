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
using System.Xml.Linq;

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
            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 0,\r\n  \"Losses\": 0,\r\n  \"WinRatio\": 0\r\n}",
                server.GetUserStats("John"));
            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 0,\r\n  \"Losses\": 0,\r\n  \"WinRatio\": 0\r\n}",
                server.GetUserStats("Thomas"));
            Assert.IsEmpty(server.GetUserStats("Martin"));

            John.Wins += 1;
            John.NumGames += 1;

            Thomas.Losses += 1;
            Thomas.NumGames += 1;

            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 1,\r\n  \"Losses\": 0,\r\n  \"WinRatio\": 100\r\n}",
                server.GetUserStats("John"));
            Assert.AreEqual("{\r\n  \"Name\": \"\",\r\n  \"Elo\": 100,\r\n  \"Wins\": 0,\r\n  \"Losses\": 1,\r\n  \"WinRatio\": 0\r\n}",
                server.GetUserStats("Thomas"));
        }

        [Test, Order(2)]
        public void GetScoreboard()
        {
            server.users["John"].Elo += 3;
            Assert.AreEqual(
                "[\r\n  {\r\n    \"Name\": \"John\",\r\n    \"Elo\": 103,\r\n    \"Wins\": 0,\r\n    \"Losses\": 0,\r\n    \"WinRatio\": 0\r\n  }," +
                "\r\n  {\r\n    \"Name\": \"Thomas\",\r\n    \"Elo\": 100,\r\n    \"Wins\": 0,\r\n    \"Losses\": 0,\r\n    \"WinRatio\": 0\r\n  }\r\n]",
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
            John.Win(John.Elo, Thomas.Elo);
            Assert.AreEqual(1, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(1, John.NumGames);
            Assert.AreEqual(116, John.Elo);

            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(0, Thomas.Losses);
            Assert.AreEqual(0, Thomas.NumGames);
            Assert.AreEqual(100, Thomas.Elo);
            Thomas.Loss(Thomas.Elo, John.Elo);
            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(1, Thomas.Losses);
            Assert.AreEqual(1, Thomas.NumGames);
            Assert.AreEqual(85, Thomas.Elo);

            John.Draw(John.Elo, Thomas.Elo);
            Assert.AreEqual(1, John.Wins);
            Assert.AreEqual(0, John.Losses);
            Assert.AreEqual(2, John.NumGames);
            Assert.AreEqual(115, John.Elo);

            Thomas.Draw(Thomas.Elo, John.Elo);
            Assert.AreEqual(0, Thomas.Wins);
            Assert.AreEqual(1, Thomas.Losses);
            Assert.AreEqual(2, Thomas.NumGames);
            Assert.AreEqual(86, Thomas.Elo);
        }

        [Test, Order(4)]
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

        [Test, Order(5)]
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

        [Test, Order(6)]
        public void CheckCardsBattle() //wihout critical hit
        {
            Card moCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b01"]; //Ork 40
            Card moCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"]; //WaterGoblin 55
            Card speCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b00"]; //FireSpell 35
            Card speCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"]; //WaterSpell 45

            Card ork2Card = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"]; //Ork 40

            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 0, false, false)); //40<55
            Assert.AreEqual(1, battle.CardsBattle(moCard2, moCard1, 0, false, false));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, moCard2, 1, false, false)); //ork 40*2 > WaterGoblin 55
            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 2, false, false));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0, false, false)); //normal > water
            Assert.AreEqual(2, battle.CardsBattle(speCard2, moCard1, 0, false, false));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 1, false, false));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 2, false, false)); //80 > 90/2
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0, false, false)); //WaterGoblin 55 vs FireSpell 35
            Assert.AreEqual(2, battle.CardsBattle(speCard1, moCard2, 0, false, false));
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 2, false, false)); //110 > 70/2
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 1, false, false));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0, false, false));
            Assert.AreEqual(1, battle.CardsBattle(speCard2, speCard1, 0, false, false));
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 1, false, false));
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 2, false, false));
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            //check if boost actually works
            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 0, false, false));
            Assert.AreEqual(0, battle.CardsBattle(ork2Card, moCard1, 0, false, false));
            Assert.AreEqual(1, battle.CardsBattle(moCard1, ork2Card, 1, false, false)); //40*2 > 40
            Assert.AreEqual(2, battle.CardsBattle(moCard1, ork2Card, 2, false, false));

            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 1, false, false)); //boost already used -> doesnt work
        }

        [Test, Order(7)]
        public void CheckCrit()
        {
            Card moCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b01"]; //Ork 40
            Card moCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"]; //WaterGoblin 55
            Card speCard1 = server.users["John"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83b00"]; //FireSpell 35
            Card speCard2 = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"]; //WaterSpell 45

            Card ork2Card = server.users["Thomas"].Deck.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"]; //Ork 40


            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 0, false, false)); // 40 < 55

            Assert.AreEqual(1, battle.CardsBattle(moCard1, moCard2, 0, true, false)); //60 > 55
            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 0, false, true)); //40 < 82,5
            Assert.AreEqual(2, battle.CardsBattle(moCard1, moCard2, 0, true, true));  // 60 < 82,5
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0, false, false)); //40*2=80 < 45/2=22,5 (normal > water)

            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0, true, false)); //120 > 22,5
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0, false, true)); //80 > 33,75
            Assert.AreEqual(1, battle.CardsBattle(moCard1, speCard2, 0, true, true));  //120 > 33,75
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0, false, false)); //55*2=110 > 35/2=17,5 (water > fire)

            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0, true, false));  //165 > 17,5
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0, false, true));  //110 > 26,25
            Assert.AreEqual(1, battle.CardsBattle(moCard2, speCard1, 0, true, true));   //165 > 26,25
            battle.boostP1Used = false;
            battle.boostP2Used = false;
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0, false, false)); //35/2=17,5 < 45*2=90 (fire < water)

            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0, true, false)); //26,25 < 90
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0, false, true)); //17,5 < 135
            Assert.AreEqual(2, battle.CardsBattle(speCard1, speCard2, 0, true, true));  //26,25 < 135

            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 0, false, false)); //40 = 40
            Assert.AreEqual(1, battle.CardsBattle(moCard1, ork2Card, 0, true, false));  //60 > 40
            Assert.AreEqual(2, battle.CardsBattle(moCard1, ork2Card, 0, false, true));  //40 < 60
            Assert.AreEqual(0, battle.CardsBattle(moCard1, ork2Card, 0, true, true));   //60 = 60
        }

        [Test, Order(8)]
        public void CheckElo()
        {
            //draw
            Assert.AreEqual(100, John.UpdateElo(John.Elo, Thomas.Elo, false, true));
            Assert.AreEqual(100, Thomas.UpdateElo(Thomas.Elo, John.Elo, false, true));
            John.Elo = 100;
            Thomas.Elo = 100;
            //john wins
            Assert.AreEqual(116, John.UpdateElo(John.Elo, Thomas.Elo, true, false));
            Assert.AreEqual(84, Thomas.UpdateElo(Thomas.Elo, John.Elo, false, false));
            John.Elo = 100;
            Thomas.Elo = 100;
            //thomas wins
            Assert.AreEqual(84, John.UpdateElo(John.Elo, Thomas.Elo, false, false));
            Assert.AreEqual(116, Thomas.UpdateElo(Thomas.Elo, John.Elo, true, false));

            John.Elo = 400;
            Thomas.Elo = 100;
            //draw
            Assert.AreEqual(389, John.UpdateElo(John.Elo, Thomas.Elo, false, true));
            Assert.AreEqual(111, Thomas.UpdateElo(Thomas.Elo, John.Elo, false, true));
            John.Elo = 400;
            Thomas.Elo = 100;
            //john wins
            Assert.AreEqual(404, John.UpdateElo(John.Elo, Thomas.Elo, true, false));
            Assert.AreEqual(96, Thomas.UpdateElo(Thomas.Elo, John.Elo, false, false));
            John.Elo = 400;
            Thomas.Elo = 100;
            //thomas wins
            Assert.AreEqual(373, John.UpdateElo(John.Elo, Thomas.Elo, false, false));
            Assert.AreEqual(127, Thomas.UpdateElo(Thomas.Elo, John.Elo, true, false));
            
        }

        [Test, Order(9)]
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

            string log = mockBattle.Object.StartBattle();
            Assert.IsNotEmpty(log);

            if (John.Wins == 1)
            {
                Console.WriteLine("John won");
                StringAssert.Contains("PLAYER 1 WON", log);
                Assert.AreEqual(1, Thomas.Losses);
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
                Assert.AreEqual(116, John.Elo);
                Assert.AreEqual(84, Thomas.Elo);
            } 
            else if (John.Losses == 1)
            {
                Console.WriteLine("thomas elo" + Thomas.Elo);
                Console.WriteLine("john elo" + John.Elo);
                Console.WriteLine("Thomas won");
                StringAssert.Contains("PLAYER 2 WON", log);
                Assert.AreEqual(1, Thomas.Wins);
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
                Assert.AreEqual(116, Thomas.Elo);
                Assert.AreEqual(84, John.Elo);
            }
            else
            {
                Console.WriteLine("Game drew");
                StringAssert.Contains("GAME ENDED IN A DRAW", log);
                Assert.AreEqual(1, John.NumGames);
                Assert.AreEqual(1, Thomas.NumGames);
                Assert.AreEqual(100, Thomas.Elo);
                Assert.AreEqual(100, John.Elo);
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
