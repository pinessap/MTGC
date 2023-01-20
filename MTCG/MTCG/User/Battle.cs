using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MTCG.Cards;
using MTCG.Cards.Enums;

[assembly: InternalsVisibleTo("MTCG-test")]

namespace MTCG.User
{
    public class Battle
    {
        internal User player1;
        internal User player2;
        private int roundCount = 0;
        public bool hasStarted = false;
        private List<string> p1CardIDs;
        private List<string> p2CardIDs;
        private int boost = 0;              //if a player has only 1 card left -> gets 2x damage boost for one round (works only once in the entire battle), 1 = player 1, 2 = player 2
        internal bool boostP1Used = false;   //check if player 1 already had a boost
        internal bool boostP2Used = false;   //check if player 2 already had a boost

        internal bool P1Crit = false;
        internal bool P2Crit = false;

        internal int? eloP1;
        internal int? eloP2;

        public Battle(User u1, User u2) // constructor
        {
            player1 = u1;
            player2 = u2;
            eloP1 = player1.Elo;
            eloP2 = player2.Elo;

            p1CardIDs = new List<string>();
            p2CardIDs = new List<string>();
            foreach (KeyValuePair<string, Card> entry in player1.Deck.cards)
            {
                p1CardIDs.Add(entry.Key);
            }
            foreach (KeyValuePair<string, Card> entry in player2.Deck.cards)
            {
                p2CardIDs.Add(entry.Key);
            }
        }

        
        public bool MoveCardFromTo(Collection fromDeck, Collection toDeck, string fromCardID) //move Card from one Deck to another 
        {
            Card fromCard = fromDeck.RemoveCard(fromCardID);    //remove card from first Deck
            if (fromCard != null)                               //check if successful
            {
                if (toDeck.AddCard(fromCard) == true)           //add that card to second Deck & check if successful
                {                                                   
                    return true;                                
                }
                Console.WriteLine("\nAdding Card failed\n");
            }
            Console.WriteLine("\n Removing Card failed\n");
            return false;                                      //return false if not successful
        }

        internal bool CheckPureMonsterFight(Card card1, Card card2) //check whether both cards are monster cards 
        {
            if (!card1.Name.ToLower().Contains("spell") && !card2.Name.ToLower().Contains("spell"))
            {
                return true;
            }
            return false;
        }

        internal int CheckSpecialties(string card1, string card2, int booster) //check special rules for fight (and determine winner of the two cards) 
        {
            if (card1.ToLower().Contains("goblin") && card2.ToLower().Contains("dragon") && booster != 1)
            {
                return 2;
            }
            else if (card2.ToLower().Contains("goblin") && card1.ToLower().Contains("dragon") && booster != 2)
            {
                return 1;
            }
            else if (card1.ToLower().Contains("wizard") && card2.ToLower().Contains("ork") && booster != 2)
            {
                return 1;
            }
            else if (card2.ToLower().Contains("wizard") && card1.ToLower().Contains("ork") && booster != 1)
            {
                return 2;
            }
            else if (card1.ToLower().Contains("knight") && card2.ToLower().Contains("waterspell") && booster != 1)
            {
                return 2;
            }
            else if (card2.ToLower().Contains("knight") && card1.ToLower().Contains("waterspell") && booster != 2)
            {
                return 1;
            }
            else if (card1.ToLower().Contains("kraken") && card2.ToLower().Contains("spell") && booster != 2)
            {
                return 1;
            }
            else if (card2.ToLower().Contains("kraken") && card1.ToLower().Contains("spell") && booster != 1)
            {
                return 2;
            }
            else if (card1.ToLower().Contains("fireelf") && card2.ToLower().Contains("dragon") && booster != 2)
            {
                return 1;
            }
            else if (card2.ToLower().Contains("fireelf") && card1.ToLower().Contains("dragon") && booster != 1)
            {
                return 2;
            }

            return 0;
        }

        internal int CardsBattle(Card card1, Card card2, int booster, bool critP1, bool critP2) //determine winner between two cards 
        {
            Console.WriteLine("Card battle");
            bool pureMoFi = CheckPureMonsterFight(card1, card2);
            int specialFight = CheckSpecialties(card1.Name, card2.Name, boost);

            if (specialFight == 0) //if no special rules apply (specialities)
            {
                double dmgP1 = card1.Damage;
                double dmgP2 = card2.Damage;

                if (booster == 1 && boostP1Used == false) 
                {
                    dmgP1 = card1.Damage * 2;
                    Console.WriteLine("boostdmg:" + dmgP1);
                    boostP1Used = true;
                } 
                else if (booster == 2 && boostP2Used == false)
                {
                    dmgP2 = card2.Damage * 2;
                    boostP2Used = true;
                }

                if(!pureMoFi)//elements play a role
                {
                    if (card1.Element == Element.Water && card2.Element == Element.Fire ||   //check if card 1 is effective against card 2 
                        card1.Element == Element.Fire && card2.Element == Element.Normal ||
                        card1.Element == Element.Normal && card2.Element == Element.Water)
                    {
                        dmgP1 *= 2;
                        dmgP2 /= 2;

                    }
                    else if (card2.Element == Element.Water && card1.Element == Element.Fire ||   //check if card 2 is effective against card 1
                             card2.Element == Element.Fire && card1.Element == Element.Normal ||
                             card2.Element == Element.Normal && card1.Element == Element.Water)
                    {
                        dmgP2 *= 2;
                        dmgP1 /= 2;
                    }
                }
                

                if (critP1) //calculate damage if player 1 gets crit
                {
                    dmgP1 *= 1.5;
                }

                if (critP2) //calculate damage if player 2 gets crit
                {
                    dmgP2 *= 1.5;
                }


                if (dmgP1 > dmgP2)
                {
                    return 1;
                }
                else if (dmgP1 < dmgP2) 
                { 
                    return 2;
                }
               
                return 0;
            }
            else //if specialities apply
            {
                return specialFight; 
            }
        }

        private void resetDecks(Collection deck1, Collection deck2) // reset decks to state before battle 
        {
            //check if deck1 has cards from deck2
            foreach (KeyValuePair<string, Card> entry in player1.Deck.cards)
            {
                if (p2CardIDs.Contains(entry.Key))
                {
                    MoveCardFromTo(deck1, deck2, entry.Key);
                }
            }

            //check if deck2 has cards from deck1
            foreach (KeyValuePair<string, Card> entry in player2.Deck.cards)
            {
                if (p1CardIDs.Contains(entry.Key))
                {
                    MoveCardFromTo(deck2, deck1, entry.Key);
                }
            }

            Console.WriteLine("Deck1 count: " + player1.Deck.cards.Count());
            Console.WriteLine("Deck2 count: " + player2.Deck.cards.Count());
        }

        private bool getCrit()
        {
            Random rand = new Random();
            int randomValue = rand.Next(1, 101);
            bool has10PercentChance = randomValue < 11;
            return has10PercentChance;
        }

        public string StartBattle() //start battle between two players 
        {
            hasStarted = true;
            Console.WriteLine("\n\n---------- START GAME ----------\n");
            string log = "---------- START GAME ----------\n";

            while (roundCount < 100) //game takes place for max. 100 rounds
            {
                //check at start of every round whether one deck is empty (check winning condition)
                if (player1.IsDeckEmpty())
                {
                    log += "---------- PLAYER 2 WON ----------\n";
                    Console.WriteLine("\n---------- PLAYER 2 WON ----------\n");
                    player2.Win(eloP2, eloP1);
                    player1.Loss(eloP1, eloP2);
                    resetDecks(player1.Deck, player2.Deck);
                    return log;
                }
                if (player2.IsDeckEmpty())
                {
                    log += "---------- PLAYER 1 WON ----------\n";
                    Console.WriteLine("\n---------- PLAYER 1 WON ----------\n");
                    player1.Win(eloP1, eloP2);
                    player2.Loss(eloP2, eloP1);
                    resetDecks(player1.Deck, player2.Deck);
                    return log;
                }

                roundCount++;

                log += "\n---------- ROUND " + roundCount + "----------\n";
                Console.WriteLine("\n---------- ROUND " + roundCount + "----------\n");

                //for every player choose random card of deck
                Card tmpCard1 = player1.ChooseRndCard();
                Card tmpCard2 = player2.ChooseRndCard();

                Console.WriteLine("Card1: " + tmpCard1.Name + " | " + tmpCard1.Element + " | " + tmpCard1.Damage);
                Console.WriteLine("Card2: " + tmpCard2.Name + " | " + tmpCard2.Element + " | " + tmpCard2.Damage);

                log += "PlayerA: " + tmpCard1.Name + " (" + tmpCard1.Damage + " Damage) vs PlayerB: " + tmpCard2.Name + " (" + tmpCard2.Damage + " Damage)\n";

                P1Crit = getCrit();
                P2Crit = getCrit();

                if (P1Crit) //calculate if Player1 gets crit
                {
                    log += "PlayerA makes a Critical Hit\n";
                    Console.WriteLine("Player 1 makes a Critical Hit\n");
                }

                if (P2Crit) //calculate if Player2 gets crit
                {
                    log += "PlayerB makes a Critical Hit\n";
                    Console.WriteLine("Player 2 makes a Critical Hit\n");
                }

                //let the two cards battle against each other
                int winner = CardsBattle(tmpCard1, tmpCard2, boost, P1Crit, P2Crit);

                P1Crit = false;
                P2Crit = false;

                if (winner == 1)
                {
                    log += "PlayerA won the round\n";
                    Console.WriteLine("Player 1 won the round\n");
                    MoveCardFromTo(player2.Deck, player1.Deck, tmpCard2.Id.ToString());
                } 
                else if (winner == 2)
                {
                    log += "PlayerB won the round\n";
                    Console.WriteLine("Player 2 won the round\n");
                    MoveCardFromTo(player1.Deck, player2.Deck, tmpCard1.Id.ToString());
                }
                else
                {
                    log += "Round ended in a Draw\n";
                    Console.WriteLine("round ended in a draw\n");
                }

                Console.WriteLine("Cards in Deck of Player 1: " + player1.Deck.cards.Count());
                Console.WriteLine("Cards in Deck of Player 2: " + player2.Deck.cards.Count());

                boost = 0;
                if (player1.Deck.cards.Count() == 1 && boostP1Used == false)
                {
                    boost = 1;
                    log += "Player 1 gets a boost for next round!\n";
                    Console.WriteLine("Player 1 gets a boost for next round!\n");
                } else if (player2.Deck.cards.Count() == 1 && boostP2Used == false)
                {
                    boost = 2;
                    log += "Player 2 gets a boost for next round!\n";
                    Console.WriteLine("Player 2 gets a boost for next round!\n");
                }
            }
            
            log += "----------GAME ENDED IN A DRAW ----------\n";
            Console.WriteLine("---------- GAME ENDED IN A DRAW ----------\n");
            player1.Draw(eloP1, eloP2);
            player2.Draw(eloP2, eloP1);
            resetDecks(player1.Deck, player2.Deck);
            return log;
        }
    }
}
