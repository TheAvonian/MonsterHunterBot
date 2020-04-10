using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Net.Models;
using System;
using System.Collections.Generic;
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
                a.Topic = "100/100";
            }).ConfigureAwait(false);
        }
    }
}
