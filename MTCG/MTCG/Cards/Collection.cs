using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    //package, deck or stack
    public class Collection
    {
        private int maxNumOfCards;

        public ConcurrentDictionary<string, Card> cards;

        public Collection() // basic constructor
        {
            cards = new ConcurrentDictionary<string, Card>();
        }
        public Collection(int maxCards) // constructor with maxNumOfCards
        {
            cards = new ConcurrentDictionary<string, Card>();
            maxNumOfCards = maxCards;
        }

        public bool AddCard(Card newCard, bool toPackage = false) //add card to dictionary
        {
            if (!toPackage) //check if card is added to package
            {
                return cards.TryAdd(newCard.Id.ToString(), newCard);
            }

            lock (cards)
            {
                if (maxNumOfCards > cards.Count)   //if package -> check if there are already 4 cards in dictionary
                {
                    return cards.TryAdd(newCard.Id.ToString(), newCard);
                }
            }
            return false;
        }

        public Card? RemoveCard(string id) //remove card from dictionary
        {
            if (cards.TryRemove(id, out var card))
            {
                return card;
            }
            return null;
        }
    }
}
