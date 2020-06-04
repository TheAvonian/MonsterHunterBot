using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MonsterHunterBot.Commands
{
    public class ShopCommands : BaseCommandModule
    {
        [Command("Shop"), Description("The shop menu for hunters to buy base weapons")]
        public async Task Shop(CommandContext ctx)
        {
            //Checks if the user has a hunter or not
            ulong uuid = ctx.Member.Id;
            if (!Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
            {
                await ctx.Channel.SendMessageAsync("You cannot shop if you're not a Hunter!");
                return;
            }
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
            var Interactivity = ctx.Client.GetInteractivity();

            List<Weapon> weaponList = GetWeaponsList();

            var NumberEmojis = new List<DiscordEmoji> {
                DiscordEmoji.FromName(ctx.Client, ":one:"),
                DiscordEmoji.FromName(ctx.Client, ":two:"),
                DiscordEmoji.FromName(ctx.Client, ":three:"),
                DiscordEmoji.FromName(ctx.Client, ":four:"),
                DiscordEmoji.FromName(ctx.Client, ":five:"),
                DiscordEmoji.FromName(ctx.Client, ":six:"),
                DiscordEmoji.FromName(ctx.Client, ":seven:"),
                DiscordEmoji.FromName(ctx.Client, ":eight:"),
                DiscordEmoji.FromName(ctx.Client, ":nine:")
            };

            string[] WeaponTypes = {"Great Sword", "Longsword", "Sword and Shield", "Dual Blades", "Hammer", "Hunter's Horn", "Lance",
            "Gun-Lance", "Switch Axe", "Charge Blade", "Insect Glaive", "Light Bowgun", "Heavy Bowgun", "Bow"};

            //TODO: Add all the different weapon type emojis
            var WeaponTypeEmojis = new List<DiscordEmoji> {
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721232408871012),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721367926964324),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721439037063189),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721533765287938),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721589566439444),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721638731939861),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721711813623859),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721755937702070),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721807825305762),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721862946979912),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721918538416268),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708721972787281960),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708722026025844829),
                DiscordEmoji.FromGuildEmote(ctx.Client, 708722079200968785)
            };

            var ShopEmbed = new DiscordEmbedBuilder()
            {
                Title = "Jason's Shop",
                Description = "Welcome to the shop! Grab some new weapons or upgrade your current one!",
                ThumbnailUrl = "https://monsterhunterworld.wiki.fextralife.com/file/Monster-Hunter-World/npc_workshop.jpg"
            };

            

            var shopDisplay = await ctx.Channel.SendMessageAsync(embed: ShopEmbed);
            bool again = true;

            //loop to allow players to go back and look at other weapon types
            while (again)
            {
                for (int i = 0; i < WeaponTypeEmojis.Count; i++)
                {
                    await shopDisplay.CreateReactionAsync(WeaponTypeEmojis[i]);
                }

                var weaponChoice = await Interactivity.WaitForReactionAsync(
                    x => x.Message == shopDisplay &&
                    x.User.Id == ctx.Member.Id &&
                    WeaponTypeEmojis.Contains(x.Emoji));

                int choiceIndex = WeaponTypeEmojis.IndexOf(weaponChoice.Result.Emoji);
                string choice = WeaponTypes[choiceIndex];

                await shopDisplay.DeleteAllReactionsAsync();

                int shopListIndex = 1;
                for (int i = 0; i < weaponList.Count; i++)
                {
                    if (weaponList[i].Rank <= hunter.Rank)
                    {
                        ShopEmbed.AddField(shopListIndex + ":", weaponList[i].ToString());
                        shopListIndex++;
                    }
                }

                await shopDisplay.ModifyAsync(embed: new Optional<DiscordEmbed>(ShopEmbed));

                List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
                for (int i = 0; i < shopListIndex - 1; i++)
                {
                    await shopDisplay.CreateReactionAsync(NumberEmojis[i]);
                    UsedEmojis.Add(NumberEmojis[i]);
                }

                DiscordEmoji UndoEmoji = DiscordEmoji.FromName(ctx.Client, ":leftwards_arrow_with_hook:");
                await shopDisplay.CreateReactionAsync(UndoEmoji);

                var item = await Interactivity.WaitForReactionAsync(
                    x => x.Message == shopDisplay &&
                    x.User.Id == ctx.Member.Id &&
                    UsedEmojis.Contains(x.Emoji) || x.Emoji == UndoEmoji);

                ShopEmbed.ClearFields();
                await shopDisplay.DeleteAllReactionsAsync();
                if (item.Result.Emoji != UndoEmoji)
                {
                    again = false;
                    int index = NumberEmojis.IndexOf(item.Result.Emoji);
                    Weapon weapon = new Weapon("something went wrong", "0", 0, 0, 0, "0");
                    //Finds the weapon that the user 'bought'
                    for (int i = 0; i < weaponList.Count; i++)
                    {
                        if (weaponList[i].ToString().Equals(ShopEmbed.Fields[index].ToString()))
                            weapon = weaponList[i];
                    }
                    hunter.Weapons.Add(weapon);
                }
                else
                {
                    await shopDisplay.ModifyAsync(embed: new Optional<DiscordEmbed>(ShopEmbed));
                }
            }
        }

        public List<Weapon> GetWeaponsList()
        {
            return new List<Weapon> { 
                new Weapon("Iron Drums", "Low level metal hunting horn", 5, 4, 1, "Hunting Horn"),
                new Weapon("Iron Glaive", "Low level metal Insect Glaive", 3, 2, 1, "Insect Glaive"),
                new Weapon("Flame Glaive", "A fiery insect glaive made from the ferocious *Rathalos*", 30, 4, 3, "Insect Glaive"),
                new Weapon("**JASON**", "Unlimited, yet somewhat forgetful, power", 92, 10, 8, "God")};
        }
    }
}
