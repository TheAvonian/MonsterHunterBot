﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MonsterHunterBot.Commands
{
    public class MonsterHunterCommands : BaseCommandModule
    {
        [Command("UpdateChannel")]
        public async Task UpdateChannel(CommandContext ctx)
        {
            await ctx.Channel.ModifyAsync(a =>
            {
                a.Name = "Griffen";
                a.Topic = new Random().Next(100).ToString() + "/100";
            });
        }

        [Command("CreateHunter"), Description("Creates the starting hunter")]
        public async Task CreateHunter(CommandContext ctx)
        {
            
            // Checks the hunter list json for already created uuid and returns if true
            string uuid = ctx.Member.GetHashCode().ToString();

            if (Bot.HunterList.Any(u => u.Uuid == uuid))
            {
                await ctx.Channel.SendMessageAsync("You bafoon! You are a Hunter already!");
                return;
            }

            // Waits and gets the user response
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendMessageAsync("What will your name be? (Max 30 characters)");

            var hName = (await GetUserMessage(ctx)).ToString();

            // If response is over 29 characters get it out of here and restart
            if(hName.Length > 29)
            {
                await ctx.Channel.SendMessageAsync("Come on dude... That's more than 30 characters.");
                return;
            }

            // Adds new hunter to the hunters list
            Bot.HunterList.Add(new ConfigHunterJson() { Hunter = new Hunter(hName), Uuid = ctx.Member.GetHashCode().ToString() });

            CreateJson();

            await ctx.Channel.SendMessageAsync("Alright, " + hName + " it is!").ConfigureAwait(false);
        }

        [Command("DeleteMyHunter"), Description("Deletes the users hunter from the database")]
        public async Task DeleteMyHunter(CommandContext ctx)
        {
            if (HasHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            await ctx.Channel.SendMessageAsync("Are you sure you wish to delete your hunter? (yes/no)");
            var userInput = (await GetUserMessage(ctx)).ToString();
            if (userInput == "yes")
            {
                Bot.HunterList.RemoveAll(u => u.Uuid == uuid);
                CreateJson();
                await ctx.Channel.SendMessageAsync("Deletion successful.");
            }
        }

        [Command("Health"), Description("Retuns how much health the hunter has")]
        public async Task Health(CommandContext ctx)
        {
            if (!HasHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.HunterList.Find(u => u.Uuid == uuid).Hunter;
            await ctx.Channel.SendMessageAsync(hunter.Name + " has " + hunter.Health + "/" + hunter.MaxHealth + " currently");
        }

        [Command("Hurt"), Description("Deals damage to the user. TESTING PURPOSES")]
        public async Task Hurt(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.HunterList.Find(u => u.Uuid == uuid).Hunter;
            hunter.TakeDamage(10);
            await UpdateDamageDisplay(hunter, ctx);
        }

        [Command("DamageDisplay")]
        public async Task DamageDisplay(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.HunterList.Find(u => u.Uuid == uuid).Hunter;
            await UpdateDamageDisplay(hunter, ctx);
        }

        public async Task UpdateDamageDisplay(Hunter hunter, CommandContext ctx)
        {
            var fullHealthRole = ctx.Guild.GetRole(698235037069606995);
            var hurtRole = ctx.Guild.GetRole(698235085857620070);
            var damagedRole = ctx.Guild.GetRole(698235116354273333);
            var nearDeathRole = ctx.Guild.GetRole(698235220373143602);

            await ctx.Member.RevokeRoleAsync(fullHealthRole);
            await ctx.Member.RevokeRoleAsync(hurtRole);
            await ctx.Member.RevokeRoleAsync(damagedRole);
            await ctx.Member.RevokeRoleAsync(nearDeathRole);

            if (hunter.Health == hunter.MaxHealth)
                await ctx.Member.GrantRoleAsync(fullHealthRole);
            else if (hunter.Health > hunter.MaxHealth / 2)
                await ctx.Member.GrantRoleAsync(hurtRole);
            else if (hunter.Health > hunter.MaxHealth / 10)
                await ctx.Member.GrantRoleAsync(damagedRole);
            else
                await ctx.Member.GrantRoleAsync(nearDeathRole);
        }

        public bool HasHunter(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            if (Bot.HunterList.Any(u => u.Uuid == uuid))
            {
                ctx.Channel.SendMessageAsync("Okay real idiot here with no hunter trying to use a hunter command...");
                return true;
            }
            return false;
        }

        public void CreateJson()
        {
            // Creates and writes to a new json file called hunters.json with all current hunters
            string json = JsonConvert.SerializeObject(Bot.HunterList, Formatting.Indented);

            File.WriteAllText("hunters.json", json);
        }

        public async Task<string> GetUserMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            return (await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member))).Result.Content;
        }

    }
}
