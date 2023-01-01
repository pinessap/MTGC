using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    public class Collection
    {
        private int maxNumOfCards;
        public ConcurrentDictionary<string, Card> cards;
        //public IReadOnlyDictionary<string, Card> Cards { get; }
        public Collection()
        {
            cards = new ConcurrentDictionary<string, Card>();
        }
        public Collection(int maxCards)
        {
            cards = new ConcurrentDictionary<string, Card>();
            maxNumOfCards = maxCards;
        }

        public bool AddCard(Card newCard, bool toPackage = false)
        {
            if (!toPackage)
            {
                return cards.TryAdd(newCard.Id.ToString(), newCard);
            }
            else if (maxNumOfCards > cards.Count)
            {
                return cards.TryAdd(newCard.Id.ToString(), newCard);
            }
            return false;
        }

        public Card RemoveCard(string id)
        {
            if (cards.TryRemove(id, out Card card))
            {
                return card;
            }
            return null;
        }
    }
}
