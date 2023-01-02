using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards;
using MTCG.Cards.Enums;

namespace MTCG.User
{
    internal class Battle
    {
        private User player1;
        private User player2;
        private int roundCount = 0;

        public Battle(User u1, User u2)
        {
            player1 = u1;
            player2 = u2;
        }

        //move Card from one Deck to another
        public bool MoveCardFromTo(Collection fromDeck, Collection toDeck, string fromCardID) 
        {
            Card fromCard = fromDeck.RemoveCard(fromCardID);    //remove card from first Deck
            if (fromCard != null)                               //check if successful
            {
                if (toDeck.AddCard(fromCard) == true)           //add that card to second Deck & check if successful
                {                                                   
                    return true;                                
                }
            }
            return false;                                      //return false ifnot  successful
        }

        private bool CheckPureMonsterFight(Card card1, Card card2)
        {
            
            if (!card1.Name.ToLower().Contains("spell") && !card2.Name.ToLower().Contains("spell"))
            {
                return true;
            }

            return false;
        }

        private int CheckSpecialties(string card1, string card2)
        {
            if (card1.ToLower().Contains("goblin") && card2.ToLower().Contains("dragon"))
            {
                return 2;
            }
            else if (card2.ToLower().Contains("goblin") && card1.ToLower().Contains("dragon"))
            {
                return 1;
            }
            else if (card1.ToLower().Contains("wizard") && card2.ToLower().Contains("ork"))
            {
                return 1;
            }
            else if (card2.ToLower().Contains("wizard") && card1.ToLower().Contains("ork"))
            {
                return 2;
            }
            else if (card1.ToLower().Contains("knight") && card2.ToLower().Contains("waterspell"))
            {
                return 2;
            }
            else if (card2.ToLower().Contains("knight") && card1.ToLower().Contains("waterspell"))
            {
                return 1;
            }
            else if (card1.ToLower().Contains("kraken") && card2.ToLower().Contains("spell"))
            {
                return 1;
            }
            else if (card2.ToLower().Contains("kraken") && card1.ToLower().Contains("spell"))
            {
                return 2;
            }
            else if (card1.ToLower().Contains("fireelf") && card2.ToLower().Contains("dragon"))
            {
                return 1;
            }
            else if (card2.ToLower().Contains("fireelf") && card1.ToLower().Contains("dragon"))
            {
                return 2;
            }

            return 0;
        }

        public int CardsBattle(Card card1, Card card2)
        {
            bool pureMoFi = CheckPureMonsterFight(card1, card2);
            int specialFight = CheckSpecialties(card1.Name, card2.Name);

            if (specialFight == 0)
            {
                if (pureMoFi)
                {
                    if (card1.Damage > card2.Damage)
                    {
                        return 1;
                    } 
                    else if (card1.Damage < card2.Damage)
                    {
                        return 2;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (card1.Element == Element.Water && card2.Element == Element.Fire ||
                        card1.Element == Element.Fire && card2.Element == Element.Normal ||
                        card1.Element == Element.Normal && card2.Element == Element.Water)
                    {
                        if (card1.Damage * 2 > card2.Damage / 2)
                        {
                            return 1;
                        }
                        else if (card1.Damage * 2 < card2.Damage / 2)
                        {
                            return 2;
                        }
                    }
                    else if (card2.Element == Element.Water && card1.Element == Element.Fire ||
                             card2.Element == Element.Fire && card1.Element == Element.Normal ||
                             card2.Element == Element.Normal && card1.Element == Element.Water)
                    {
                        if (card2.Damage * 2 > card1.Damage / 2)
                        {
                            return 2;
                        }
                        else if (card2.Damage * 2 < card1.Damage / 2)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (card1.Damage > card2.Damage)
                        {
                            return 1;
                        }
                        else if (card1.Damage < card2.Damage)
                        {
                            return 2;
                        }
                    }
                }

                return 0;
            }
            else
            {
                return specialFight;
            }
        }

        public bool StartBattle()
        {
            Console.WriteLine("\n\n---------- START GAME ----------\n");

            while (roundCount <= 100)
            {
                if (player1.IsDeckEmpty())
                {
                    Console.WriteLine("\n---------- PLAYER 2 WON ----------\n");
                    player2.Win();
                    player1.Loss();
                    return true;
                }
                if (player2.IsDeckEmpty())
                {
                    Console.WriteLine("\n---------- PLAYER 1 WON ----------\n");
                    player1.Win();
                    player2.Loss();
                    return true;
                }
                roundCount++;
                Console.WriteLine("\n---------- ROUND " + roundCount + "----------\n");

                Card tmpCard1 = player1.ChooseRndCard();
                Card tmpCard2 = player1.ChooseRndCard();

                Console.WriteLine("Card1: " + tmpCard1.Name + " | " + tmpCard1.Element + " | " + tmpCard1.Damage);
                Console.WriteLine("Card2: " + tmpCard2.Name + " | " + tmpCard2.Element + " | " + tmpCard2.Damage);

                int winner = CardsBattle(tmpCard1, tmpCard2);

                if (winner == 1)
                {
                    Console.WriteLine("Player 1 won the round\n");
                    MoveCardFromTo(player2.Deck, player1.Deck, tmpCard2.Id.ToString());
                } 
                else if (winner == 2)
                {
                    Console.WriteLine("Player 2 won the round\n");
                    MoveCardFromTo(player1.Deck, player2.Deck, tmpCard1.Id.ToString());
                }
                else
                {
                    Console.WriteLine("round ended in a draw\n");
                }

                Console.WriteLine("Cards in Deck of Player 1: " + player1.Deck.cards.Count());
                Console.WriteLine("Cards in Deck of Player 2: " + player2.Deck.cards.Count());
            }
            
            player1.Draw();
            player2.Draw();
            return true;
        }
    }
}
