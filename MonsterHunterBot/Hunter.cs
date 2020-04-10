using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterHunterBot
{
    public class Hunter
    {
        public string Name { get; set; }
        public List<string> Weapons { get; private set; }
        public List<string> ResourcePouch { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; } = 100;
        //Make armor class && make empty slot and initialize with that instead of null

        public Hunter(string name)
        {
            this.Name = name;
            Weapons = new List<string>();
            ResourcePouch = new List<string>();
            Health = MaxHealth;
        }

        //Reduces the health of the player and updates their damage display
        public void TakeDamage(int damage)
        {
            Health -= damage;

            if (Health < 0)
                Health = 0;
        }

        //heals the player with the value entered
        public int Heal(int value)
        {
            Health += value;

            //Ensures health doesn't exceed maximum
            if (Health > MaxHealth)
                Health = MaxHealth;
            return Health;
        }

        //Swaps out old armor for new 
        /*public void EquipArmor(int slotIndex, string newArmor)
        {
            //sets the old armors "equipped" boolean to false
            Armor.GetValue(slotIndex);

            Armor.SetValue(newArmor, slotIndex);

            //TODO: update "equipped" field to true
        }*/
    }
}
