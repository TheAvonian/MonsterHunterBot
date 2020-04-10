using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterHunterBot
{
    public class Monster
    {
        public string Name { get; private set; }
        public int Health { get; private set; }
        //Represents the chance for a monster to get critted on
        public int CritChance { get; private set; }
        public int Rank { get; private set; }
        public List<string> MoveList { get; private set; } //TODO make a "Moves" class and change this to a list of moves

        public Monster(string name, int health, int rank, int critChance)
        {
            this.Name = name;
            this.Health = health;
            this.Rank = rank;
            this.CritChance = critChance;
        }

        public void Attack(Hunter Hunter)
        {
            Hunter.TakeDamage(new Random().Next(1, 3));
        }

        //Edits the monster's health the correct amount and returns a true if it got hit and false if it dodged
        public Boolean TakeDamage(int Damage)
        {
            int AvoidChance = new Random().Next(1, 100);
            if (AvoidChance < 20)
            {
                return false;
            }
            else
            {
                Health -= Damage;

                if (Health < 0)
                    Health = 0;

                return true;
            }
        }
    }
}
