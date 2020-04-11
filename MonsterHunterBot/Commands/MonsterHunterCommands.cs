using DSharpPlus;
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
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MonsterHunterBot.Commands
{
    public class MonsterHunterCommands : BaseCommandModule
    {

        [Command("BeginMH")]
        public async Task BeginMH(CommandContext ctx)
        {
            var dedicateMessage = await ctx.Channel.SendMessageAsync("Are you sure you want to dedicate this channel to Monster Hunter?");

            var Interactivity = ctx.Client.GetInteractivity();
            var thumbsUp = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDown = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await dedicateMessage.CreateReactionAsync(thumbsUp);
            await dedicateMessage.CreateReactionAsync(thumbsDown);
            await Task.Delay(100);

            var reaction = await Interactivity.WaitForReactionAsync(
                    x => x.Message == dedicateMessage && x.User.Id == ctx.Member.Id &&
                    (x.Emoji == thumbsUp || x.Emoji == thumbsDown ), TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            if (reaction.TimedOut)
            {
                await dedicateMessage.ModifyAsync("Timed out");
                return;
            }
            else if (reaction.Result.Emoji == thumbsDown)
            {
                await dedicateMessage.ModifyAsync("Okay then.");
                return;
            }
            else
            {
                Directory.CreateDirectory(".\\Servers\\" + ctx.Guild.Id + "\\Hunters");
                Directory.CreateDirectory(".\\Servers\\" + ctx.Guild.Id + "\\Monsters");

                new ConfigMonsterJson() { ActiveMonster = new Monster("Empty Monster", 0, 0, 0, ctx.Guild) };

                Bot.ServerHunterList[ctx.Guild.Id] = new List<ConfigHunterJson>();

                await dedicateMessage.ModifyAsync("Done!");
            }
        }

        [Command("CreateHunter"), Description("Creates the starting hunter")]
        public async Task CreateHunter(CommandContext ctx)
        {
            
            // Checks the hunter list json for already created uuid and returns if true
            string uuid = ctx.Member.GetHashCode().ToString();

            if (Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
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
            Bot.ServerHunterList[ctx.Guild.Id].Add(configHunter);

            UpdateJson(ctx);

            await ctx.Channel.SendMessageAsync("Alright, " + hName + " it is!").ConfigureAwait(false);
            await UpdateDamageDisplay(configHunter.Hunter, ctx);
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
                DeleteJson(ctx);
                Bot.ServerHunterList[ctx.Guild.Id].RemoveAll(u => u.Uuid == uuid);
                await ctx.Channel.SendMessageAsync("Deletion successful.");
            }
        }

        [Command("Health"), Description("Retuns how much health the hunter has")]
        public async Task Health(CommandContext ctx)
        {
            if (NoHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
            await ctx.Channel.SendMessageAsync(hunter.Name + " has " + hunter.Health + "/" + hunter.MaxHealth + " currently");
        }

        [Command("Hurt"), Description("Deals damage to the user. TESTING PURPOSES")]
        public async Task Hurt(CommandContext ctx, int damage)
        {
            if (NoHunter(ctx)) return;
            string uuid = ctx.Member.GetHashCode().ToString();
            Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter.TakeDamage(damage);
            
            await UpdateDamageDisplay(Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter, ctx);
            UpdateJson(ctx);
        }

        [Command("DamageDisplay")]
        public async Task DamageDisplay(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
            await UpdateDamageDisplay(hunter, ctx);
        }

        [Command("SpawnMonster")]
        public async Task SpawnMonster(CommandContext ctx)
        { 
            //Checks that a monster isnt already active

            Monster Jagras = new Monster("Jagras", 25, 1, 2, ctx.Guild);
            await UpdateChannel(ctx, Jagras);

            var MonsterEmbed = new DiscordEmbedBuilder
            {
                Title = "A Jagras has arrived!",
                ThumbnailUrl = "https://vignette.wikia.nocookie.net/monsterhunter/images/3/39/MHW-Jagras_Icon.png/revision/latest/scale-to-width-down/170?cb=20180128024205",
                Color = DiscordColor.Red
            };

            await ctx.Channel.SendMessageAsync(embed:MonsterEmbed);

            //wait until health drops to 0
            
        }

        [Command("DisplayArmor")]
        public async Task DisplayArmor(CommandContext ctx)
        {
            string uuid = ctx.Member.GetHashCode().ToString();
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
            var Interactivity = ctx.Client.GetInteractivity();

            var HelmetEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370227997507594);
            var ChestplateEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370277641289738);
            var WaistEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370363079524382);
            var GreavesEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370396021587978);
            var BracersEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370329512378531);
              
            var ArmorEmbed = new DiscordEmbedBuilder
            {
                Title = hunter.Name + "'s Armor Loadout",
                Color = DiscordColor.Blue,
                Description = "React to view each armor slot"
            };

            var ArmorDisplay = await ctx.Channel.SendMessageAsync(embed: ArmorEmbed);
            await ArmorDisplay.CreateReactionAsync(HelmetEmoji);
            await ArmorDisplay.CreateReactionAsync(ChestplateEmoji);
            await ArmorDisplay.CreateReactionAsync(BracersEmoji);
            await ArmorDisplay.CreateReactionAsync(WaistEmoji);
            await ArmorDisplay.CreateReactionAsync(GreavesEmoji);
            await Task.Delay(100);

            //Sends the embed, adds reactions, and resends based on choice
            while (true)
            {
                var reaction = await Interactivity.WaitForReactionAsync(
                    x => x.Message == ArmorDisplay && x.User.Id == ctx.Member.Id && 
                    (x.Emoji == HelmetEmoji || x.Emoji == ChestplateEmoji || x.Emoji == BracersEmoji || x.Emoji == WaistEmoji || x.Emoji == GreavesEmoji), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                if (reaction.Result is null)
                {
                    await ctx.Channel.DeleteMessageAsync(ArmorDisplay);
                    break;
                }
                
                Armor equipment = null;
                if (ArmorEmbed.Title != hunter.Name + "'s Helmet Slot" && reaction.Result.Emoji == HelmetEmoji)
                {
                    equipment = hunter.ArmorSlots[0];
                    ArmorEmbed.Title = hunter.Name + "'s Helmet Slot";
                }
                else if (ArmorEmbed.Title != hunter.Name + "'s Chest Slot" && reaction.Result.Emoji == ChestplateEmoji)
                {
                    equipment = hunter.ArmorSlots[1];
                    ArmorEmbed.Title = hunter.Name + "'s Chest Slot";
                }
                else if (ArmorEmbed.Title != hunter.Name + "'s Bracers Slot" && reaction.Result.Emoji == BracersEmoji)
                {
                    equipment = hunter.ArmorSlots[2];
                    ArmorEmbed.Title = hunter.Name + "'s Bracers Slot";
                }
                else if (ArmorEmbed.Title != hunter.Name + "'s Waist Slot" && reaction.Result.Emoji == WaistEmoji)
                {
                    equipment = hunter.ArmorSlots[3];
                    ArmorEmbed.Title = hunter.Name + "'s Waist Slot";
                }
                else if (ArmorEmbed.Title != hunter.Name + "'s Greaves Slot" && reaction.Result.Emoji == GreavesEmoji)
                {
                    equipment = hunter.ArmorSlots[4];
                    ArmorEmbed.Title = hunter.Name + "'s Greaves Slot";
                }
                else 
                    continue;

                ArmorEmbed.ClearFields();

                ArmorEmbed.Description = equipment.Name;
                ArmorEmbed.AddField("Defense", equipment.Defense.ToString());
                ArmorEmbed.AddField("Description", equipment.Description);

                await ArmorDisplay.ModifyAsync(default, new Optional<DiscordEmbed>(ArmorEmbed));
            }
        }

        public async Task UpdateChannel(CommandContext ctx, Monster Monster)
        {
            await ctx.Channel.ModifyAsync(a =>
            {
                a.Name = Monster.Name;
                a.Topic = Monster.Health + "/" + Monster.MaxHealth;
            });
            //When monster health drops to 0 revert channel back to normal
            
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
            if (!Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
            {
                ctx.Channel.SendMessageAsync("Okay real idiot here with no hunter trying to use a hunter command...");
                return true;
            }
            return false;
        }

        public void UpdateJson(CommandContext ctx)
        {
            File.WriteAllText(".\\Servers\\" + ctx.Guild.Id + "\\Hunters\\" + Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.GetHashCode().ToString()).Uuid + ".json", JsonConvert.SerializeObject(Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.GetHashCode().ToString()), Formatting.Indented));
        }

        public void DeleteJson(CommandContext ctx)
        {
            File.Delete(".\\Servers\\" + ctx.Guild.Id + "\\Hunters\\" + Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.GetHashCode().ToString()).Uuid + ".json");
        }

        public async Task<string> GetUserMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            return (await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member))).Result.Content;
        }

    }
}
