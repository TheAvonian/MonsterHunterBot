using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonsterHunterBot
{
    class ShopCommands
    {
        public async Task OpenShop(CommandContext ctx)
        {
            var Interactivity = ctx.Client.GetInteractivity();
            //Stores the calling users hunter
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == ctx.User.Id).Hunter;

            //Creates the embed
            var ShopEmbed = new DiscordEmbedBuilder()
            {
                Title = "Atlas' Weapon Shop",
                Description = "Use a reaction to select the weapon type...",
                Color = DiscordColor.Cyan
            };
            ShopEmbed.WithAuthor(ctx.User.Username, ctx.User.AvatarUrl);

            //A list of all the reactions for weapon types -> TODO
            List<DiscordEmoji> weaponTypeEmojis = new List<DiscordEmoji>
            {

            };

            //Sends and stores the message
            var ShopMessage = await ctx.Channel.SendMessageAsync(embed: ShopEmbed);

            //Watches and waits for reactions until the message times out
            while(true)
            {
                var reaction = await Interactivity.WaitForReactionAsync(
                    x => x.Message == ShopMessage &&
                    x.User.Id == ctx.Member.Id &&
                    weaponTypeEmojis.Contains(x.Emoji)).ConfigureAwait(false);

                if (reaction.TimedOut)
                {
                    await ShopMessage.DeleteAsync();
                    return;
                }

                //After selecting a weapon type generate numbered reactions for each of the base weapons available along with a back button
            }
        }
    }
}
