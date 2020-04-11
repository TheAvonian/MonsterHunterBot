using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterHunterBot
{
    public class Armor
    {
        public string Name { get; private set; }
        public int Defense { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; }
        public Boolean Equipped { get; private set; } = false;

        public Armor(string name, int defense, string description, string icon)
        {
            Name = name;
            Defense = defense;
            Description = description;
            Icon = icon;
        }

        public Armor(string name, int defense)
        {
            Name = name;
            Defense = defense;
        }

        public Armor()
        {
            Name = "Empty Slot";
            Defense = 0;
            Description = "It's just nothing...";
            Equipped = true;
        }

        public void EquipArmor()
        {
            Equipped = true;
        }

        public void UnequipArmor()
        {
            Equipped = false;
        }
    }
}
