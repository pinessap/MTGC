using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Cards.Enums;

namespace MTCG.Cards
{
    public class Card
    {
        public Guid Id { get; }
        public string Name { get; }
        public double Damage { get; }
        Element Element { get; }

        public Card(string name, double damage, Element element, string id)
        {
            Id = new Guid(id);
            Name = name;
            Damage = damage;
            Element = element;
        }

        public static Card CreateCard(string name, double damage, string id)
        {
            string lowerName = name.ToLower();

            Enums.Element element = Element.Normal;
            if (lowerName.Contains("fire"))
            {
                element = Element.Fire;
            } 
            else if (lowerName.Contains("water"))
            {
                element = Element.Water;
            }

            if (lowerName.Contains("spell"))
            {
                return new SpellCard(name, damage, element, id);
            }

            Monster monsterType = Monster.Goblin;
            if (lowerName.Contains("Troll"))
            {
                monsterType = Monster.Troll;
            } 
            else if (lowerName.Contains("Dragon"))
            {
                monsterType = Monster.Dragon;
            } 
            else if (lowerName.Contains("Wizard"))
            {
                monsterType = Monster.Wizard;
            }
            else if (lowerName.Contains("Ork"))
            {
                monsterType = Monster.Ork;
            }
            else if (lowerName.Contains("Knight"))
            {
                monsterType = Monster.Knight;
            }
            else if (lowerName.Contains("Kraken"))
            {
                monsterType = Monster.Kraken;
            }
            else if (lowerName.Contains("Elf"))
            {
                monsterType = Monster.Elf;
            }
            return new MonsterCard(name, damage, element, monsterType, id);
        }
    }
}
