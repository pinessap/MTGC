using MTCG.Cards.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards
{
    internal class MonsterCard : Card
    {
        public Guid Id { get; }
        public string Name { get; }
        public double Damage { get; }
        public Element Element { get; }
        public Monster MonsterType { get; }
        public MonsterCard(string name, double damage, Element element, Monster monsterType, string id) : base (name, damage, element, id)
        {
            //Id = new Guid(id);
            /*Name = name;
            Damage = damage;
            Element = element;*/
            MonsterType = monsterType;
        }
    }
}
