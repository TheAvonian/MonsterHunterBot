using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace MonsterHunterBot
{
    public class Monster
    {
        public static Monster Empty { get => new Monster(); }
        public string Name { get; private set; }
        public int Health { get; private set; }
        //Represents the chance for a monster to get critted on
        public int CritChance { get; private set; }
        public int Rank { get; private set; }
        public List<Moves> MoveList { get; private set; } = new List<Moves>(); //TODO make a "Moves" class and change this to a list of moves
        public int MaxHealth { get; }
        //The Hunters that the Monster would want to attack
        public Dictionary<ulong, Hunter> Targets { get; private set; } = new Dictionary<ulong, Hunter>();
        public List<ulong> UuidList { get; private set; } = new List<ulong>();
        public string LastHit { get; private set; }
        public int LastDamage { get; private set; }

        public Monster() { Name = "Empty"; Health = 0; CritChance = 0; Rank = 0; MaxHealth = 0; }
        public Monster(string name, int maxHealth, int rank, int critChance, DiscordGuild discordGuild)
        {
            Name = name;
            Health = maxHealth;
            MaxHealth = maxHealth;
            Rank = rank;
            CritChance = critChance;

            int numOfHunters = Bot.ServerHunterList[discordGuild.Id].Count;
            if(numOfHunters != 0)
            {
                //Picks a random hunter from the server's hunter list and adds them to the targets array
                ConfigHunterJson primaryTarget = Bot.ServerHunterList[discordGuild.Id][new Random().Next(0, numOfHunters)];
                Targets.Add(primaryTarget.Uuid, primaryTarget.Hunter);
                UuidList.Add(primaryTarget.Uuid);
            }


            //TODO prep lists of moves per monster rank and pull them from Json files instead of this .add bs
            if(rank == 1)
            {

                MoveList.Add(new Moves("Slash", 3, 1, "The " + Name + " reaches out with its claws and drags them through your chest!", 2));
                MoveList.Add(new Moves("Bite", 5, 2, "The " + Name + " jumps at you and its teeth pierce your armor!", 3));
            }
            
        }

        //Attacks a random target with a random move and returns the move it used
        public Moves Attack()
        {
            //choose a random user id from the targets list to attack
            ulong uuid = UuidList[new Random().Next(0, Targets.Count)];
            Hunter attackTarget = Targets[uuid];

            //Chooses a random move to use on the target
            Moves move = MoveList[new Random().Next(0, MoveList.Count - 1)];

            attackTarget.TakeDamage(move);
            return move;
        }

        //Edits the monster's health the correct amount and returns a true if it got hit and false if it dodged
        public Boolean TakeDamage(int damage, CommandContext ctx)
        {
            //previous targets length
            int targetsOldLength = Targets.Count;

            //Finds the uuid of the hunter to add them to the target pool
            ulong uuid = ctx.Member.Id;
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;

            //Adds the person who hit the monster to the target pool
            Targets[uuid] = hunter;

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
