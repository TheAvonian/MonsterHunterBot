using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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
            var configHunter = new ConfigHunterJson() { Hunter = new Hunter(hName), Uuid = ctx.Member.GetHashCode().ToString() };
            Bot.HunterList.Add(configHunter);

            UpdateJson(configHunter);

            await ctx.Channel.SendMessageAsync("Alright, " + hName + " it is!").ConfigureAwait(false);
        }

        [Command("DeleteMyHunter"), Description("Deletes the users hunter from the database")]
        public async Task DeleteMyHunter(CommandContext ctx)
        {
            if (NoHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            await ctx.Channel.SendMessageAsync("Are you sure you wish to delete your hunter? (yes/no)");
            var userInput = (await GetUserMessage(ctx)).ToString();
            if (userInput == "yes")
            {
                DeleteJson(Bot.HunterList.Find(u => u.Uuid == uuid));
                Bot.HunterList.RemoveAll(u => u.Uuid == uuid);
                await ctx.Channel.SendMessageAsync("Deletion successful.");
            }
        }

        [Command("Health"), Description("Retuns how much health the hunter has")]
        public async Task Health(CommandContext ctx)
        {
            if (NoHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.HunterList.Find(u => u.Uuid == uuid).Hunter;
            await ctx.Channel.SendMessageAsync(hunter.Name + " has " + hunter.Health + "/" + hunter.MaxHealth + " currently");
        }

        [Command("Hurt"), Description("Deals damage to the user. TESTING PURPOSES")]
        public async Task Hurt(CommandContext ctx, int damage)
        {
            if (NoHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            Bot.HunterList.Find(u => u.Uuid == uuid).Hunter.TakeDamage(damage);
            
            await UpdateDamageDisplay(Bot.HunterList.Find(u => u.Uuid == uuid).Hunter, ctx);
            UpdateJson(Bot.HunterList.Find(u => u.Uuid == uuid));
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
            var deadRole = ctx.Guild.GetRole(698273743239118919);
            int newRoleIndex;

            if (hunter.Health == hunter.MaxHealth)
            { await ctx.Member.GrantRoleAsync(fullHealthRole); newRoleIndex = 1; }
            else if (hunter.Health > hunter.MaxHealth / 2)
            { await ctx.Member.GrantRoleAsync(hurtRole); newRoleIndex = 2; }
            else if (hunter.Health > hunter.MaxHealth / 10)
            { await ctx.Member.GrantRoleAsync(damagedRole); newRoleIndex = 3; }
            else if (hunter.Health > 0)
            { await ctx.Member.GrantRoleAsync(nearDeathRole); newRoleIndex = 4; }
            else
            { await ctx.Member.GrantRoleAsync(deadRole); newRoleIndex = 5; }

            if (newRoleIndex != 1)
                await ctx.Member.RevokeRoleAsync(fullHealthRole);
            if (newRoleIndex != 2)
                await ctx.Member.RevokeRoleAsync(hurtRole);
            if (newRoleIndex != 3)
                await ctx.Member.RevokeRoleAsync(damagedRole);
            if (newRoleIndex != 4)
                await ctx.Member.RevokeRoleAsync(nearDeathRole);
            if (newRoleIndex != 5)
                await ctx.Member.RevokeRoleAsync(deadRole);

        }

        public bool NoHunter(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            if (!Bot.HunterList.Any(u => u.Uuid == uuid))
            {
                ctx.Channel.SendMessageAsync("Okay real idiot here with no hunter trying to use a hunter command...");
                return true;
            }
            return false;
        }

        public void UpdateJson(ConfigHunterJson chj)
        {
            File.WriteAllText(".\\Hunters\\" + chj.Uuid + ".json", JsonConvert.SerializeObject(chj, Formatting.Indented));
        }

        public void DeleteJson(ConfigHunterJson chj)
        {
            File.Delete(".\\Hunters\\" + chj.Uuid + ".json");
        }

        public async Task<string> GetUserMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            return (await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member))).Result.Content;
        }

    }
}
