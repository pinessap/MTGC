using Moq;
using MTCG.DB;
using MTCG.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_test
{
    public class TradingTest
    {
        private ServerMethods server;
        private Mock<IDatabase> mockDB;

        private string jsonPckg1;
        private string jsonPckg2;

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
        }


        [Test, Order(1)]
        public void CreateDeal()
        {
            Assert.IsEmpty(server.GetTradings("John")); //empty because no deals available

            mockDB.Setup(x => x.CreateTrade(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<double>(), It.IsAny<string>())).Returns(true);

            Assert.AreEqual(-2, server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                                                 "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                                                 Environment.NewLine +
                                                                 "\"CardToTrade\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                                                 Environment.NewLine +
                                                                 "\"Type\": \"monster\"," + Environment.NewLine +
                                                                 "\"MinimumDamage\": 15" + Environment.NewLine +
                                                                 "}"
            )); //-2 because user doesn't own card with that id

            Assert.AreEqual(0, server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                                                "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                                                Environment.NewLine +
                                                                "\"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                                                Environment.NewLine +
                                                                "\"Type\": \"monster\"," + Environment.NewLine +
                                                                "\"MinimumDamage\": 15" + Environment.NewLine +
                                                                "}"
            )); //0 because should work

            Assert.AreEqual(-1, server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                                                 "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," +
                                                                 Environment.NewLine +
                                                                 "\"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                                                 Environment.NewLine +
                                                                 "\"Type\": \"monster\"," + Environment.NewLine +
                                                                 "\"MinimumDamage\": 15" + Environment.NewLine +
                                                                 "}"
            )); //-1 because trade id already exists

            mockDB.Verify(
                s => s.CreateTrade(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(),
                    It.IsAny<string>()), Times.AtLeastOnce);

            Assert.IsNotEmpty(server.GetTradings("John"));
            Assert.IsNotEmpty(server.GetTradings("Thomas"));
            Assert.IsNotEmpty(server.tradingDeals);
        }

        [Test, Order(2)]
        public void ShowDeals()
        {
            server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                             "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," + Environment.NewLine +
                                             "\"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                             Environment.NewLine +
                                             "\"Type\": \"monster\"," + Environment.NewLine +
                                             "\"MinimumDamage\": 15" + Environment.NewLine +
                                             "}"
            );

            Assert.AreEqual("[" + Environment.NewLine + "  {" + Environment.NewLine +
                            "    \"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," + Environment.NewLine +
                            "    \"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," + Environment.NewLine +
                            "    \"Type\": \"monster\"," + Environment.NewLine +
                            "    \"MinimumDamage\": 15.0" + Environment.NewLine +
                            "  }" + Environment.NewLine + "]", server.GetTradings("John"));

            Assert.AreEqual("[" + Environment.NewLine + "  {" + Environment.NewLine +
                            "    \"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," + Environment.NewLine +
                            "    \"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," + Environment.NewLine +
                            "    \"Type\": \"monster\"," + Environment.NewLine +
                            "    \"MinimumDamage\": 15.0" + Environment.NewLine +
                            "  }" + Environment.NewLine + "]", server.GetTradings("Thomas"));
            
            Assert.IsNotEmpty(server.tradingDeals);
        }

        [Test, Order(3)]
        public void DeleteDeal()
        {
            server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                             "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," + Environment.NewLine +
                                             "\"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                             Environment.NewLine +
                                             "\"Type\": \"monster\"," + Environment.NewLine +
                                             "\"MinimumDamage\": 15" + Environment.NewLine +
                                             "}"
            );

            Assert.IsNotEmpty(server.GetTradings("John"));

            mockDB.Setup(x => x.DeleteTrade(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Assert.AreEqual(-2,
                server.DeleteTradingDeal("John", "3fa85f64-5717-4562-b3fc-2c963f66afz0")); //-2 because id doesn't exist
            Assert.AreEqual(-1,
                server.DeleteTradingDeal("Thomas",
                    "3fa85f64-5717-4562-b3fc-2c963f66afa6")); //-1 because thomas isn't owner of trade

            Assert.AreEqual(0,
                server.DeleteTradingDeal("John", "3fa85f64-5717-4562-b3fc-2c963f66afa6")); //deleting works

            mockDB.Verify(s => s.DeleteTrade(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            Assert.IsEmpty(server.GetTradings("John")); //empty because no deals available
            Assert.IsEmpty(server.tradingDeals);
        }

        [Test, Order(4)]
        public void MakeDeal()
        {
            server.CreateTradingDeal("John", "{" + Environment.NewLine +
                                             "\"Id\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"," + Environment.NewLine +
                                             "\"CardToTrade\": \"845f0dc7-37d0-426e-994e-43fc3ac83b00\"," +
                                             Environment.NewLine +
                                             "\"Type\": \"monster\"," + Environment.NewLine +
                                             "\"MinimumDamage\": 15" + Environment.NewLine +
                                             "}"
            );

            Assert.IsNotEmpty(server.GetTradings("John"));
            Assert.IsNotEmpty(server.GetTradings("Thomas"));

            mockDB.Setup(x => x.PerformTrade(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Assert.AreEqual(-2, server.TradeCard("John", "3fa85f64-5717-4562-b3fc-2c963f66afz0", "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"")); //-2 tradeid does not exist
            Assert.AreEqual(-3, server.TradeCard("John", "3fa85f64-5717-4562-b3fc-2c963f66afa6", "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"")); //-3 trade with oneself
            Assert.AreEqual(-1, server.TradeCard("Thomas", "3fa85f64-5717-4562-b3fc-2c963f66afa6", "\"845f0dc7-37d0-426e-994e-43fc3ac83c00\"")); //-1 user does not have card in stack
            Assert.AreEqual(-1, server.TradeCard("Thomas", "3fa85f64-5717-4562-b3fc-2c963f66afa6", "\"845f0dc7-37d0-426e-994e-43fc3ac83c06\"")); //-1 card does not fulfill requirements

            Assert.AreEqual(0, server.TradeCard("Thomas", "3fa85f64-5717-4562-b3fc-2c963f66afa6", "\"845f0dc7-37d0-426e-994e-43fc3ac83c08\"")); //0 trade is sucessful

            mockDB.Verify(s => s.PerformTrade(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);

            Assert.True(server.users["John"].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));
            Assert.False(server.users["Thomas"].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));
            Assert.True(server.users["Thomas"].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83b00"));
            Assert.False(server.users["John"].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83b00"));

            Assert.IsEmpty(server.GetTradings("John"));
            Assert.IsEmpty(server.GetTradings("Thomas"));
            Assert.IsEmpty(server.tradingDeals);
        }
    }
}
