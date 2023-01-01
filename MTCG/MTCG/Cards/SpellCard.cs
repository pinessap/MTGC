using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards.Enums;

namespace MTCG.Cards
{
    internal class SpellCard : Card
    {
        public Guid Id { get; }
        public string Name { get; }
        public double Damage { get; }
        public Element Element { get; }

        public SpellCard(string name, double damage, Element element, string id) : base(name, damage, element, id)
        {
            //Id = new Guid(id);
            /*Name = name;
            Damage = damage;
            Element = element;*/
        }
    }
}
