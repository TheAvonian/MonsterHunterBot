using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
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
        public static int MaxHealth;
        //The Hunters that the Monster would want to attack
        public Dictionary<string, Hunter> Targets { get; private set; }
        public List<string> UuidList { get; private set; }
        

        public Monster(string name, int maxHealth, int rank, int critChance, DiscordGuild discordGuild)
        {
            Name = name;
            Health = MaxHealth;
            MaxHealth = maxHealth;
            Rank = rank;
            CritChance = critChance;

            int numOfHunters = Bot.ServerHunterList[discordGuild.Id].Count;
            //Picks a random hunter from the server's hunter list and adds them to the targets array
            ConfigHunterJson primaryTarget = Bot.ServerHunterList[discordGuild.Id][new Random().Next(0, numOfHunters)];
            Targets.Add(primaryTarget.Uuid, primaryTarget.Hunter);
            UuidList.Add(primaryTarget.Uuid);
        }


        public void Attack()
        {
            //choose a random user id from the targets list to attack
            string uuid = UuidList[new Random().Next(0, Targets.Count)];

            Hunter attackTarget = Targets[uuid];

            attackTarget.TakeDamage(new Random().Next(1, 3));
        }

        //Edits the monster's health the correct amount and returns a true if it got hit and false if it dodged
        public Boolean TakeDamage(int damage, CommandContext ctx)
        {
            //previous targets length
            int targetsOldLength = Targets.Count;

            //Finds the uuid of the hunter to add them to the target pool
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;

            //Adds the person who hit the monster to the target pool
            Targets.Add(uuid, hunter);

            //if dictionary didnt get a new value then its a duplicate and dont add the uuid to list
            if(Targets.Count > targetsOldLength)
            {
                UuidList.Add(uuid);
            }

            int AvoidChance = new Random().Next(1, 100);
            if (AvoidChance < 20)
            {
                return false;
            }
            else
            {
                Health -= damage;

                if (Health < 0)
                    Health = 0;

                return true;
            }
        }
    }
}
