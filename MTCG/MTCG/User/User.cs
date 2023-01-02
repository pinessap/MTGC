using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;

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
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int NumGames { get; set; }
        public Collection Stack { get; set; }
        public Collection Deck { get; set; }

        //numofgames

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
            Wins = 0;
            Losses = 0;
            NumGames = 0;
            Stack = new Collection();
            Deck = new Collection();
        }

        public bool MoveCardToStack(string id)
        {
            if (!Deck.cards.ContainsKey(id))
            {
                return false;
            }
            return Stack.AddCard(Deck.RemoveCard(id));
        }

        public bool MoveCardToDeck(string id)
        {
            if (!Stack.cards.ContainsKey(id))
            {
                return false;
            }
            return Deck.AddCard(Stack.RemoveCard(id));
        }

        public void Draw()
        {
            NumGames++;
        }

        public void Win()
        {
            NumGames++;
            Wins++;
            Elo += 3;
        }

        public void Loss()
        {
            NumGames++;
            Losses++;
            Elo -= 5;
        }

        public Card ChooseRndCard()
        {
            Random random = new Random();
            int index = random.Next(Deck.cards.Count());
            Card card = Deck.cards.Values.ElementAt(index);
            return card;
        }

        public bool IsDeckEmpty()
        {
            if (Deck.cards.IsEmpty || Deck.cards.Count() == 0 || !Deck.cards.Any())
            {
                return true;
            }
            return false;
        }
    }
}
