using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonsterHunterBot
{
    public class AttackEmbed
    {
        public CommandContext Ctx { get; private set; }
        public Hunter Hunter { get; private set; }
        public DiscordEmbedBuilder Embed { get; private set; }
        public int TimeLeftOnCooldown { get; set; }
        public string DamageDescription { get; set; }
        public int DamageTakenFromHit { get; set; }
        public DiscordMessage Message { get; private set; }

        public AttackEmbed(CommandContext ctx)
        {
            Ctx = ctx;
            Hunter = Bot.ServerHunterList[Ctx.Guild.Id].Find(u => u.Uuid == Ctx.Member.Id).Hunter;
            Embed = new DiscordEmbedBuilder { };

            Embed.WithTitle(Hunter.Name + "'s Attack Panel");
            Embed.WithAuthor(Ctx.User.Username, default, Ctx.User.GetAvatarUrl(ImageFormat.Png));
            Embed.WithColor(DiscordColor.Rose);
            Embed.WithFooter("00:00");

            Embed.AddField("Damage Log...", "\u200b");
            for (int i = 0; i < Hunter.CurrentWeapon.MoveSet.Count; i++)
            {
                string fieldName = "**" + (i + 1) + ":**";
                string moveInfo = Hunter.CurrentWeapon.MoveSet[i].toString();
                Embed.AddField(fieldName, moveInfo);
            }
        }

        public void SetMessage(DiscordMessage message)
        {
            Message = message;
        }

        public async Task UpdateSequence()
        {
            while (Bot.ServerActiveMonster[Ctx.Guild.Id].Monster.Health > 0)
            {
                if (!(DamageDescription is null) && DamageDescription.StartsWith('*'))
                    DamageDescription = DamageDescription.Substring(1, DamageDescription.Length - 2);
                else
                    DamageDescription = null;

                TimeLeftOnCooldown--;
                if (TimeLeftOnCooldown < 0)
                    TimeLeftOnCooldown = 0;

                Embed.WithFooter(ClockFormatOfCooldown(TimeLeftOnCooldown));
                if (DamageDescription is null)
                {
                    Embed.Fields[0].Name = "Damage Log..";
                    Embed.Fields[0].Value = "\u200b";
                }
                else
                {
                    Embed.Fields[0].Name = DamageDescription;
                    Embed.Fields[0].Value = DamageTakenFromHit.ToString();
                }
                await Message.ModifyAsync(embed: new Optional<DiscordEmbed>(Embed));
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public bool CanUseMove()
        {
            if (TimeLeftOnCooldown != 0)
                return true;
            return false;
        }

        public string ClockFormatOfCooldown(int cooldown)
        {
            string cooldownTime = "";
            int minutes = cooldown / 60;
            if (minutes < 10)
                cooldownTime += "0" + minutes;
            else
                cooldownTime += minutes;
            int seconds = cooldown % 60;
            cooldownTime += ":";
            if (seconds < 10)
                cooldownTime += "0" + seconds;
            else
                cooldownTime += seconds;

            return cooldownTime;
        }
    }
}
