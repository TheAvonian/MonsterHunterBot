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
        public List<string> Weapons { get; private set; }
        public List<string> ResourcePouch { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; } = 100;
        public Armor[] ArmorSlots { get; private set; } = { new Armor(), new Armor(), new Armor(), new Armor(), new Armor() };
        public Weapons CurrentWeapon { get; private set; } = new Weapons("Fists", "Your wife beaters...", 1, 0, 0, "Fists");
        public GuildCard GuildCard { get; set; }
        public Palico Palico { get; set; }
        public string LastHitTaken { get; set; }
        public string LastDamageTaken { get; set; }

        public Hunter(string name)
        {
            this.Name = name;
            Weapons = new List<string>();
            ResourcePouch = new List<string>();
            Health = MaxHealth;

            GuildCard = new GuildCard();
        }

        //Reduces the health of the player and updates their damage display
        public void TakeDamage(Moves moveUsed)
        {
            int damage = moveUsed.GenerateDamage();
            Health -= damage;
            LastHitTaken = moveUsed.Description;
            LastDamageTaken = "The monster hit you for " + damage;
            WatchDamageReport();


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

        public async Task WatchDamageReport()
        {
            string originalHit = LastHitTaken;

            int countdown = 2;
            while (countdown > 0)
            {
                //if the hit taken has changed since called
                if (originalHit != LastHitTaken)
                    return;
                countdown--;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            LastHitTaken = null;
        }
    }
}
