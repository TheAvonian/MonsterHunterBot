using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace MonsterHunterBot
{
    public class Hunter
    {
        public string Name { get; set; }
        public List<Weapons> Weapons { get; private set; }
        public List<string> ResourcePouch { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; } = 100;
        public Armor[] ArmorSlots { get; private set; } = { new Armor(), new Armor(), new Armor(), new Armor(), new Armor() };
        public Weapons CurrentWeapon { get; private set; } = new Weapons("Fists", "Your wife beaters...", 1, 0, 0, "Fists");
        public GuildCard GuildCard { get; set; }
        public Palico Palico { get; set; }
        public AttackEmbed AttackEmbed { get; set; }
        public int Rank { get; set; }

        public Hunter(string name)
        {
            this.Name = name;
            Weapons = new List<string>();
            ResourcePouch = new List<string>();
            Health = MaxHealth;
            Rank = 1;

            GuildCard = new GuildCard();
        }

        //Reduces the health of the player and updates their damage display
        public void TakeDamage(Moves moveUsed)
        {
            int damage = moveUsed.GenerateDamage();
            Health -= damage;

            if (AttackEmbed != null)
            {
                AttackEmbed.DamageDescription = "**" + moveUsed.Description + "**";
                AttackEmbed.DamageTakenFromHit = damage;
            }
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
        public void EquipArmor(int slotIndex, Armor newArmor)
        {
            //sets the old armors "equipped" boolean to false
            ArmorSlots[slotIndex].UnequipArmor();

            ArmorSlots[slotIndex] = newArmor;

            ArmorSlots[slotIndex].EquipArmor();
        }
    }
}
