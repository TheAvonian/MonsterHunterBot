using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MonsterHunterBot
{
    public class Moves
    {
        public string Name { get; private set; }
        public int DamageMax { get; private set; }
        public int DamageMin { get; private set; }
        public string Description { get; private set; }

        public Moves(string name, int damageMax, int damageMin, string description)
        {
            Name = name;
            DamageMax = damageMax;
            DamageMin = damageMin;
            Description = description;
        }

        public int generateDamage() // Perhaps moves should have their own additonal crit chance or lack there of
        {
            int damage = new Random().Next(DamageMin, DamageMax);

            return damage;
        }

    }
}
