using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterHunterBot
{
    public abstract class Items
    {
        public string Name { get;  private set; }
        public int Value { get; private set; }
        public string Rarity { get; private set; }
        public string Description { get; private set; }
        public string Icon { get; private set; }

        public Items(string Name, string desc, string rarity, int value, string icon)
        {
            this.Name = Name;
            this.Description = desc;
            this.Rarity = rarity;
            this.Value = value;
            this.Icon = icon;
        }

        public Items(string name, string rarity, int value)
        {
            this.Name = name;
            this.Rarity = rarity;
            this.Value = value;
        }
    }
}
