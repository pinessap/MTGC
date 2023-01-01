using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MTCG.Server
{
    internal class Data
    {
        private ConcurrentDictionary<string, User.User> users;

        private static readonly Data serverData = new Data();
        public static Data ServerData
        {
            get { return serverData; }
        }
        private Data()
        {
            users = new ConcurrentDictionary<string, User.User>();
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
                return JsonConvert.SerializeObject(jsonObject);
                //return token;
            }
            return string.Empty;
        }

        public string GetUserData(string username, string token)
        {
            JObject jsonObject = new JObject { { "Name", users[username].Name }, { "Bio", users[username].Bio }, { "Image", users[username].Image } };
            return JsonConvert.SerializeObject(jsonObject);
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



    }
}
