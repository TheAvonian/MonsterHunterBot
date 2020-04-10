﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

            var response = await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member));

            var hName = response.Result.Content;

            // If response is over 29 characters get it out of here and restart
            if(hName.Length > 29)
            {
                await ctx.Channel.SendMessageAsync("Come on dude... That's more than 30 characters.");
                return;
            }

            // Adds new hunter to the hunters list
            Bot.HunterList.Add(new ConfigHunterJson() { HunterName = hName, Uuid = ctx.Member.GetHashCode().ToString() });

            CreateJson();

            await ctx.Channel.SendMessageAsync("Alright, " + hName + " it is!");
        }

        public void CreateJson()
        {
            // Creates and writes to a new json file called hunters.json with all current hunters
            string json = JsonConvert.SerializeObject(Bot.HunterList, Formatting.Indented);

            File.WriteAllText("hunters.json", json);
        }

        [Command("DeleteMyHunter"), Description("Deletes the users hunter from the database")]
        public async Task DeleteMyHunter(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            await ctx.Channel.SendMessageAsync("Are you sure you wish to delete your hunter? (yes/no)");
            var interactivity = ctx.Client.GetInteractivity();
            if ((await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member))).Result.Content == "yes")
            {
                Bot.HunterList.RemoveAll(u => u.Uuid == uuid);
                CreateJson();
                await ctx.Channel.SendMessageAsync("Deletion successful.");
            }
        }
    }
}
