using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace MonsterHunterBot
{
    public abstract class Weapons
    {
        public string Name { get; private set; }
        public int AttackDamage { get; private set; }
        public string Description { get; private set; }
        public int CritChance { get; private set; }
        public List<Moves> MoveSet { get; private set; } 
        public int Rank { get; private set; } //Represents the weapons average rank it is effective against
        public string WeaponType { get; private set; }

        readonly string[] WeaponTypes = {"Great Sword", "Longsword", "Sword and Shield", "Dual Blades", "Hammer", "Hunter's Horn", "Lance",
            "Gun-Lance", "Switch Axe", "Charge Blade", "Insect Glaive", "Light Bowgun", "Heavy Bowgun", "Bow", "Fists" };

        public Weapons(string name, string description, int critChance, int rank, string weaponType)
        {
            Name = name;
            Description = description;
            CritChance = critChance;
            Rank = rank;
            WeaponType = weaponType;

            if(WeaponType == "Fists")
            {
                MoveSet.Add(new Moves("Punch", 1, 1, "Uppercuts, jabs, strikes, its all just a punch really"));
            }
        }

        public int Attack(Moves move)
        {
            int damage = move.generateDamage();

            //Handle crit chance
            double random = new Random().NextDouble() * 100;

            if (random <= CritChance)
                damage = Convert.ToInt32(Math.Round(damage * 1.25));

            return damage;
        }
    }
}
