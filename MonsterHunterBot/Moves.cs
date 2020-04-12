using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public int Cooldown { get; private set; }

        public Moves(string name, int damageMax, int damageMin, string description, int cooldown)
        {
            Name = name;
            DamageMax = damageMax;
            DamageMin = damageMin;
            Description = description;
            Cooldown = cooldown;
        }

        public int GenerateDamage() // Perhaps moves should have their own additonal crit chance or lack there of
        {
            int damage = new Random().Next(DamageMin, DamageMax);

            return damage;
        }

        public string toString()
        {
            string cooldownTime = "";
            int minutes = Cooldown / 60;
            if (minutes < 10)
                cooldownTime += "0" + minutes;
            else
                cooldownTime += minutes;
            int seconds = Cooldown % 60;
            cooldownTime += ":";
            if (seconds < 10)
                cooldownTime += "0" + seconds;
            else
                cooldownTime += seconds;

            return "**Name: ***" + Name + "*    **Damage: **" + DamageMin + "-" + DamageMax + "    **Cooldown: **" + cooldownTime + "\n**Description: **" + Description;
        }
    }
}
