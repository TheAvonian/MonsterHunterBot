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

namespace MonsterHunterBot
{
    public static class HelpingMethods
    {
        // Prompts the user with a yes or no reaction message with the given message
        public static async Task<bool> GetYesNo(CommandContext ctx, string message)
        {
            var question = await ctx.Channel.SendMessageAsync(message);

            var Interactivity = ctx.Client.GetInteractivity();
            var thumbsUp = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDown = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await question.CreateReactionAsync(thumbsUp);
            await question.CreateReactionAsync(thumbsDown);

            var reaction = await Interactivity.WaitForReactionAsync(x => x.Message == question && x.User.Id == ctx.Member.Id && (x.Emoji == thumbsUp || x.Emoji == thumbsDown), TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            return reaction.TimedOut ? false : reaction.Result.Emoji == thumbsUp;
        }
    }
}
