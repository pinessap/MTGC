using Moq;
using MTCG.Cards;
using MTCG.DB;
using MTCG.Server;
using MTCG.User;

namespace MTCG_test
{
    public class PackagesTests
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
        }

        [Test, Order(1)]
        public void CreatePackage()
        {
            mockDB.Setup(x => x.CreatePackage(It.IsAny<Collection>())).Returns(true);
            Assert.True(server.AddPackage(json));
            Assert.False(server.AddPackage(json)); //adding same cards should fail
            mockDB.Verify(s => s.CreatePackage(It.IsAny<Collection>()), Times.Once);

            Assert.True(server.CardIDs.Count == 5);
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83c04", server.CardIDs.ElementAt(0));
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83c05", server.CardIDs.ElementAt(1));
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83c06", server.CardIDs.ElementAt(2));
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83c07", server.CardIDs.ElementAt(3));
            Assert.AreEqual("845f0dc7-37d0-426e-994e-43fc3ac83c08", server.CardIDs.ElementAt(4));

            Assert.True(server.packages.Count == 1);
            Assert.True(server.packages.ElementAt(0).cards.Count == 5);

            Assert.True(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c04"));
            Assert.True(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c05"));
            Assert.True(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c06"));
            Assert.True(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c07"));
            Assert.True(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));

            Assert.False(server.packages.ElementAt(0).cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c01")); //shouldn't have random id in it

            Assert.AreEqual("FireSpell",
                server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c04"].Name);
            Assert.AreEqual(35, server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c04"].Damage);

            Assert.AreEqual("Ork", server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Name);
            Assert.AreEqual(40, server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Damage);

            Assert.AreEqual("WaterSpell",
                server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Name);
            Assert.AreEqual(45, server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Damage);

            Assert.AreEqual("Dragon", server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Name);
            Assert.AreEqual(50, server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Damage);

            Assert.AreEqual("WaterGoblin",
                server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Name);
            Assert.AreEqual(55, server.packages.ElementAt(0).cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Damage);
        }

        [Test, Order(2)]
        public void AcquirePackage()
        {
            Assert.AreEqual(0, server.BuyPackage(username)); //0 because no package exists
            Assert.AreEqual(20, server.users[username].Coins);

            server.AddPackage(json);

            server.users[username].Coins -= 16;
            Assert.AreEqual(4, server.users[username].Coins);
            Assert.AreEqual(-1, server.BuyPackage(username));

            server.users[username].Coins += 16;
            Assert.AreEqual(20, server.users[username].Coins);

            mockDB.Setup(x => x.BuyPackage(It.IsAny<string>(), It.IsAny<Collection>())).Returns(true);
            Assert.AreEqual(1, server.BuyPackage(username));
            mockDB.Verify(s => s.BuyPackage(It.IsAny<string>(), It.IsAny<Collection>()), Times.Once);

            Assert.AreEqual(15, server.users[username].Coins);
            Assert.AreEqual(0, server.BuyPackage(username)); //0 because no package exists


            Assert.AreEqual(5, server.users[username].Stack.cards.Count);

            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c04"));
            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c05"));
            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c06"));
            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c07"));
            Assert.True(server.users[username].Stack.cards.ContainsKey("845f0dc7-37d0-426e-994e-43fc3ac83c08"));

            Assert.AreEqual("FireSpell",
                server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c04"].Name);
            Assert.AreEqual(35, server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c04"].Damage);

            Assert.AreEqual("Ork", server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Name);
            Assert.AreEqual(40, server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c05"].Damage);

            Assert.AreEqual("WaterSpell",
                server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Name);
            Assert.AreEqual(45, server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c06"].Damage);

            Assert.AreEqual("Dragon", server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Name);
            Assert.AreEqual(50, server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c07"].Damage);

            Assert.AreEqual("WaterGoblin",
                server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Name);
            Assert.AreEqual(55, server.users[username].Stack.cards["845f0dc7-37d0-426e-994e-43fc3ac83c08"].Damage);
        }
    }
}