using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MTCG.Cards;
using MTCG.Cards.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG.User
{
    public class Trading
    {
        public Guid Id { get; }
        public Card CardToTrade { get; }
        public string WantedType { get; }
        public double WantedDamage { get; }
        public string Owner { get; }

        public Trading(string id, Card cardToTrade, string type, double damage, string owner) // constructor
        {
            Id = new Guid(id);
            WantedType = type;
            WantedDamage = damage;
            CardToTrade = cardToTrade;
            Owner = owner;
        }

    }
}
