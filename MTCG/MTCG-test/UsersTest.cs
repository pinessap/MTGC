using MTCG.User;
using MTCG.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using MTCG.Cards;
using MTCG.DB;
using Newtonsoft.Json.Bson;

namespace MTCG_test
{
    public class UsersTest
    {
        private ServerMethods server;
        private Mock<IDatabase> mockDB;

        [SetUp]
        public void Setup()
        {
            mockDB = new Mock<IDatabase>();
            /*mockDB.Setup(x => x.LoadUsers()).Verifiable();
            mockDB.Setup(x => x.LoadCardIDs()).Verifiable();
            mockDB.Setup(x => x.LoadPackages()).Verifiable();
            mockDB.Setup(x => x.LoadCards()).Verifiable();
            mockDB.Setup(x => x.LoadTradings(It.IsAny<ConcurrentDictionary<string, Card>>())).Verifiable();*/
            
            server = new ServerMethods(true,mockDB.Object);
            /*mockDB.Verify(s => s.LoadUsers(), Times.Once);
            mockDB.Verify(s => s.LoadCardIDs(), Times.Once);
            mockDB.Verify(s => s.LoadPackages(), Times.Once);
            mockDB.Verify(s => s.LoadCards(), Times.Once);
            mockDB.Verify(s => s.LoadTradings(It.IsAny<ConcurrentDictionary<string, Card>>()), Times.Once);*/
        }

        [Test, Order(1)]
        public void AddNewUser()
        {
            string json = "{" + Environment.NewLine +
                          "\"Username\": \"John\"," + Environment.NewLine +
                          "\"Password\": \"Adams\"" + Environment.NewLine +
                          "}";

            mockDB.Setup(x => x.CreateUser(It.IsAny<User>())).Returns(true);
            Assert.True(server.RegisterUser(json));
            Assert.False(server.RegisterUser(json)); //adding user with same username should fail
            mockDB.Verify(s => s.CreateUser(It.IsAny<User>()), Times.Once);

            Assert.True(server.users.ContainsKey("John"));
            Assert.AreEqual("John", server.users["John"].Username);
            Assert.AreEqual("Adams", server.users["John"].Password);
        }

        [Test, Order(2)]
        public void AddUserData()
        {
            server.RegisterUser("{" + Environment.NewLine +
                                "\"Username\": \"John\"," + Environment.NewLine +
                                "\"Password\": \"Adams\"" + Environment.NewLine +
                                "}");

            string username = "John";
            string json = "{" + Environment.NewLine +
                          "\"Name\": \"johnny\"," + Environment.NewLine +
                          "\"Bio\": \"do not disturb\"," + Environment.NewLine +
                          "\"Image\": \":/\"" + Environment.NewLine +
                          "}";

            mockDB.Setup(x => x.UpdateUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            Assert.True(server.UpdateUserData(username, json));
            Assert.False(server.UpdateUserData("Martin", json)); //user does not exist
            mockDB.Verify(s => s.CreateUser(It.IsAny<User>()), Times.AtLeastOnce);

            Assert.AreEqual("johnny", server.users["John"].Name);
            Assert.AreEqual("do not disturb", server.users["John"].Bio);
            Assert.AreEqual(":/", server.users["John"].Image);
        }

        [Test, Order(3)]
        public void GetUserToken()
        {
            string json = "{" + Environment.NewLine +
                          "\"Username\": \"John\"," + Environment.NewLine +
                          "\"Password\": \"Adams\"" + Environment.NewLine +
                          "}";

            string failJson = "{" + Environment.NewLine +
                          "\"Username\": \"Martin\"," + Environment.NewLine +
                          "\"Password\": \"Kent\"" + Environment.NewLine +
                          "}";

            server.RegisterUser(json);
            Assert.AreEqual("{\r\n  \"token\": \"John-mtcgToken\"\r\n}", server.GetToken(json));
            Assert.AreEqual(string.Empty, server.GetToken(failJson));
        }

        [Test, Order(4)]
        public void GetUserData()
        {
            string regJson = "{" + Environment.NewLine +
                          "\"Username\": \"John\"," + Environment.NewLine +
                          "\"Password\": \"Adams\"" + Environment.NewLine +
                          "}";

            string dataJson = "{" + Environment.NewLine +
                          "\"Name\": \"johnny\"," + Environment.NewLine +
                          "\"Bio\": \"do not disturb\"," + Environment.NewLine +
                          "\"Image\": \":/\"" + Environment.NewLine +
                          "}";

            string username = "John";
            string token = "John-mtcgToken";

            server.RegisterUser(regJson);
            server.UpdateUserData(username, dataJson);

            Assert.AreEqual("{" + Environment.NewLine +
                            "  \"Name\": \"johnny\"," + Environment.NewLine +
                            "  \"Bio\": \"do not disturb\"," + Environment.NewLine +
                            "  \"Image\": \":/\"" + Environment.NewLine +
                            "}",
                server.GetUserData(username, token));
            
            Assert.AreEqual(string.Empty, server.GetUserData("Martin", "Martin-mtcgToken"));
        }
    }
}
