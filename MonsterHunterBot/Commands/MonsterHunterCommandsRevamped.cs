using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonsterHunterBot.Commands
{
    class MonsterHunterCommandsRevamped : BaseCommandModule
    {
        [Command("Begin"), Description("Begins the slippery slope into the world of Monster Hunter")]
        public async Task Begin(CommandContext ctx)
        {
            bool dedicateChannelResponse = await HelpingMethods.GetYesNo(ctx, "Do you wish to use this channel for the Monster Hunter bot?");

            if (!dedicateChannelResponse)
            {
                await ctx.Channel.SendMessageAsync("No it is then...");
                return;
            }


        }

    }
}
