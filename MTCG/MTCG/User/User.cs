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
        public string Password { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Bio { get; set; }
        public int? Coins { get; set; }
        public int? Elo { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? NumGames { get; set; }
        public Collection Stack { get; set; }
        public Collection Deck { get; set; }

        public string latestBattleLog = string.Empty;

        public User() // constructor
        {
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

        private int GetKValue(int? player1Elo, int? player2Elo)
        {
            if (!player1Elo.HasValue || !player2Elo.HasValue)
            {
                return 0;
            }

            int eloDifference = Math.Abs(player1Elo.Value - player2Elo.Value);

            if (eloDifference <= 400)
            {
                return 32;
            }
            else if (eloDifference <= 800)
            {
                return 24;
            }
            else
            {
                return 16;
            }
        }
        
        internal int UpdateElo(int? playerElo, int? opponentElo, bool playerWon, bool isDraw)
        {
            if (!playerElo.HasValue || !opponentElo.HasValue)
            {
                return 0;
            }
            int K = GetKValue(playerElo, opponentElo);
            //Console.WriteLine("K:" + K);
            //Console.WriteLine("player: " + playerElo);
            //Console.WriteLine("opponent: " + opponentElo);
            double expectedScore = 1 / (1 + Math.Pow(10, (opponentElo.Value - playerElo.Value) / 400.0));
            //Console.WriteLine("expected:" + expectedScore);
            if (isDraw)
            {
                return playerElo.Value + (int)(K * (0.5 - expectedScore));
            }
            else
            {
                //Console.WriteLine(playerElo.Value + (int)(K * ((playerWon ? 1 : 0) - expectedScore)));
                return playerElo.Value + (int)(K * ((playerWon ? 1 : 0) - expectedScore));
            }
        }

        public void Draw(int? playerElo, int? opponentElo) //increase number of games played by user 
        {
            NumGames++;
            Elo = UpdateElo(playerElo, opponentElo, false, true);
        }

        public void Win(int? playerElo, int? opponentElo) //increase NumGames, Wins and Elo 
        {
            NumGames++;
            Wins++;
            //Elo += 3;
            Elo = UpdateElo(playerElo, opponentElo, true, false);
        }

        public void Loss(int? playerElo, int? opponentElo) //increase NumGames, Losses and decrease Elo 
        {
            NumGames++;
            Losses++;
            //Elo -= 5;
            Elo = UpdateElo(playerElo, opponentElo, false, false);
        }

        public Card ChooseRndCard() //choose random card from Deck 
        {
            Random random = new Random();
            int index = Helpers.RandomNumberGenerator.Next(Deck.cards.Count());
            //int index = random.Next(Deck.cards.Count());
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
