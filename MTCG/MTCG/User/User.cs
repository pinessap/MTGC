using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.User
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; }
        public string Token { get; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Bio { get; set; }
        public int Coins { get; set; }
        public int Elo { get; set; }



        //stack
        //deck
        //elo
        //numofgames
        //wins
        //losses
        //coins

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Token = username + "-mtcgToken";
            Name = "";
            Bio = "";
            Image = "";
            Coins = 20;
            Elo = 100;


        }

       
    }
}
