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

        public bool isBattling = false;
        public string latestBattleLog = string.Empty;

        public User(string username, string password) // constructor
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

        public bool MoveCardToStack(string id) //move card from Deck to Stack 
        {
            if (!Deck.cards.ContainsKey(id))
            {
                return false;
            }
            return Stack.AddCard(Deck.RemoveCard(id));
        }

        public bool MoveCardToDeck(string id) //move card from Stack to Deck 
        {
            if (!Stack.cards.ContainsKey(id))
            {
                return false;
            }
            return Deck.AddCard(Stack.RemoveCard(id));
        }

        public void Draw() //increase number of games played by user 
        {
            NumGames++;
        }

        public void Win() //increase NumGames, Wins and Elo 
        {
            NumGames++;
            Wins++;
            Elo += 3;
        }

        public void Loss() //increase NumGames, Losses and decrease Elo 
        {
            NumGames++;
            Losses++;
            Elo -= 5;
        }

        public Card ChooseRndCard() //choose random card from Deck 
        {
            Random random = new Random();
            int index = random.Next(Deck.cards.Count());
            Card card = Deck.cards.Values.ElementAt(index);
            return card;
        }

        public bool IsDeckEmpty() //check if any cards are left in Deck 
        {
            if (Deck.cards.IsEmpty || Deck.cards.Count() == 0 || !Deck.cards.Any())
            {
                return true;
            }
            return false;
        }
    }
}
