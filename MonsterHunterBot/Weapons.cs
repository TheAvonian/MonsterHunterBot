using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace MonsterHunterBot
{
    public class Weapons
    {
        public string Name { get; private set; }
        public int BaseDamage { get; private set; }
        public string Description { get; private set; }
        public int CritChance { get; private set; }
        public List<Moves> MoveSet { get; private set; } = new List<Moves>();
        public int Rank { get; private set; } //Represents the weapons average rank it is effective against
        public string WeaponType { get; private set; }

        readonly string[] WeaponTypes = {"Great Sword", "Longsword", "Sword and Shield", "Dual Blades", "Hammer", "Hunter's Horn", "Lance",
            "Gun-Lance", "Switch Axe", "Charge Blade", "Insect Glaive", "Light Bowgun", "Heavy Bowgun", "Bow", "Fists" };

        public Weapons(string name, string description, int baseDamage, int critChance, int rank, string weaponType)
        {
            Name = name;
            Description = description;
            BaseDamage = baseDamage;
            CritChance = critChance;
            Rank = rank;
            WeaponType = weaponType;

            if(WeaponType == "Fists")
            {
                MoveSet.Add(new Moves("Punch", 0, 0, "Uppercuts, jabs, strikes, its all just a punch really", 2));
            }
        }

        public int Attack(Moves move)
        {
            int damage = BaseDamage + move.GenerateDamage();

            //Handle crit chance
            int random = new Random().Next(0, 100);

            if (random <= CritChance)
                damage = Convert.ToInt32(Math.Round(damage * 1.25));

            return damage;
        }
    }
}
