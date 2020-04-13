using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                    x => x.Message == dedicateMessage && x.User.Id == ctx.Member.Id && ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator) &&
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
            else if(reaction.Result.Emoji == thumbsUp)
            {
                Directory.CreateDirectory(".\\Servers\\" + ctx.Guild.Id + "\\Hunters");
                Directory.CreateDirectory(".\\Servers\\" + ctx.Guild.Id + "\\Monsters");

                Bot.ServerActiveMonster[ctx.Guild.Id] = new ConfigMonsterJson() { Monster = Monster.Empty };
                UpdateMonster(ctx, false);
                Bot.ServerHunterList[ctx.Guild.Id] = new List<ConfigHunterJson>();

                await dedicateMessage.ModifyAsync("Done!");
            }
        }

        [Command("CreateHunter"), Description("Creates the starting hunter")]
        public async Task CreateHunter(CommandContext ctx)
        {
            
            // Checks the hunter list json for already created uuid and returns if true
            ulong uuid = ctx.Member.Id;

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

            var playerRole = await ctx.Guild.CreateRoleAsync(hName, Permissions.None, new DiscordColor("00FF00"), false, false);
            // Adds new hunter to the hunters list
            var configHunter = new ConfigHunterJson() { Hunter = new Hunter(hName), Uuid = ctx.Member.Id, Role =  playerRole };
            configHunter.Hunter.GuildCard.Path = ".\\Servers\\" + ctx.Guild.Id + "\\Hunters\\" + ctx.Member.Id + ".jpg";
            Bot.ServerHunterList[ctx.Guild.Id].Add(configHunter);

            UpdateHunterJson(ctx);
            await ctx.Member.GrantRoleAsync(playerRole);
            await ctx.Channel.SendMessageAsync("Alright, " + hName + " it is!").ConfigureAwait(false);
            await UpdateDamageDisplay(ctx);
        }

        [Command("DeleteMyHunter"), Description("Deletes the users hunter from the database")]
        public async Task DeleteMyHunter(CommandContext ctx)
        {
            if (NoHunter(ctx)) return;
            ulong uuid = ctx.Member.Id;
            await ctx.Channel.SendMessageAsync("Are you sure you wish to delete your hunter? (yes/no)");
            var userInput = (await GetUserMessage(ctx)).ToString();
            if (userInput == "yes")
            {
                DeleteHunterJson(ctx);
                await Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Role.DeleteAsync();
                Bot.ServerHunterList[ctx.Guild.Id].RemoveAll(u => u.Uuid == uuid);
                await ctx.Channel.SendMessageAsync("Deletion successful.");
            }
        }

        [Command("Health"), Description("Retuns how much health the hunter has")]
        public async Task Health(CommandContext ctx)
        {
            if (NoHunter(ctx)) return;
            ulong uuid = ctx.Member.Id;
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
            await ctx.Channel.SendMessageAsync(hunter.Name + " has " + hunter.Health + "/" + hunter.MaxHealth + " currently");
        }

        [Command("Hurt"), Description("Deals damage to the user. TESTING PURPOSES")]
        public async Task Hurt(CommandContext ctx, int damage)
        {
            if (NoHunter(ctx)) return;
            ulong uuid = ctx.Member.Id;
            Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter.TakeDamage(damage);
            
            await UpdateDamageDisplay(ctx);
            UpdateHunterJson(ctx);
        }

        [Command("DamageDisplay")]
        public async Task DamageDisplay(CommandContext ctx)
        {
            await UpdateDamageDisplay(ctx);
        }

        [Command("SpawnMonster")]
        public async Task SpawnMonster(CommandContext ctx)
        {
            //Checks that a monster isnt already active
            if (Bot.ServerActiveMonster[ctx.Guild.Id].Monster.Health > 0)
            {
                await ctx.Channel.SendMessageAsync("A monster is already active! Stop it stupid devs. Yeah I know its you... This is a testing only command so who else could it be");
                return;
            }


            Monster Jagras = new Monster("Jagras", 5, 1, 2, ctx.Guild);

            var MonsterEmbed = new DiscordEmbedBuilder
            {
                Title = "A Jagras has arrived!",
                ThumbnailUrl = "https://vignette.wikia.nocookie.net/monsterhunter/images/3/39/MHW-Jagras_Icon.png/revision/latest/scale-to-width-down/170?cb=20180128024205",
                Color = DiscordColor.Red
            };

            await ctx.Channel.SendMessageAsync(embed:MonsterEmbed);

            Bot.ServerActiveMonster[ctx.Guild.Id] = new ConfigMonsterJson() { Monster = Jagras };
            UpdateMonster(ctx, true);

            await MonsterAttacks(ctx);

        }

        [Command("HunterDisplay")]
        public async Task HunterDisplay(CommandContext ctx)
        {
            ulong uuid = ctx.Member.Id;
            if (Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
            {


                Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
                var Interactivity = ctx.Client.GetInteractivity();

                var HelmetEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370227997507594);
                var ChestplateEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370277641289738);
                var WaistEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370363079524382);
                var GreavesEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370396021587978);
                var BracersEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698370329512378531);
                var StatsEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698752321127055380);
                var SwordEmoji = DiscordEmoji.FromGuildEmote(ctx.Client, 698753308470280213);

                var HunterEmbed = new DiscordEmbedBuilder
                {
                    Title = hunter.Name,
                    Color = DiscordColor.DarkGreen,
                    Description = "Select the reactions to get info on your Hunter!",
                };

                var HunterDisplay = await ctx.Channel.SendMessageAsync(embed: HunterEmbed);
                await HunterDisplay.CreateReactionAsync(StatsEmoji);
                await HunterDisplay.CreateReactionAsync(SwordEmoji);
                await HunterDisplay.CreateReactionAsync(HelmetEmoji);
                await HunterDisplay.CreateReactionAsync(ChestplateEmoji);
                await HunterDisplay.CreateReactionAsync(BracersEmoji);
                await HunterDisplay.CreateReactionAsync(WaistEmoji);
                await HunterDisplay.CreateReactionAsync(GreavesEmoji);
                await Task.Delay(100);

                //Sends the embed, adds reactions, and resends based on choice
                while (true)
                {
                    var reaction = await Interactivity.WaitForReactionAsync(
                        x => x.Message == HunterDisplay && x.User.Id == ctx.Member.Id &&
                        (x.Emoji == HelmetEmoji || x.Emoji == ChestplateEmoji || x.Emoji == BracersEmoji || x.Emoji == WaistEmoji
                        || x.Emoji == GreavesEmoji || x.Emoji == SwordEmoji || x.Emoji == StatsEmoji)).ConfigureAwait(false);
                    if (reaction.TimedOut)
                    {
                        await ctx.Channel.DeleteMessageAsync(HunterDisplay);
                        break;
                    }

                    await HunterDisplay.DeleteReactionAsync(reaction.Result.Emoji, reaction.Result.User);

                    HunterEmbed.ClearFields();
                    if (HunterEmbed.Title != hunter.Name + "'s Weapon Slot" && reaction.Result.Emoji == SwordEmoji)
                    {
                        HunterEmbed.Title = "" + hunter.Name + "'s Weapon Slot";
                        HunterEmbed.Description = "***" + hunter.CurrentWeapon.Name + "***";
                        HunterEmbed.AddField("Description: ", hunter.CurrentWeapon.Description);
                        HunterEmbed.AddField("Weapon Type: ", hunter.CurrentWeapon.WeaponType);
                        HunterEmbed.AddField("Rank: ", hunter.CurrentWeapon.Rank.ToString());
                        HunterEmbed.AddField("Move List: ", hunter.CurrentWeapon.MoveSet.ToString());
                        HunterEmbed.AddField("Crit Chance: ", hunter.CurrentWeapon.CritChance.ToString());
                    }
                    else if (HunterEmbed.Title != hunter.Name + "'s Stats" && reaction.Result.Emoji == StatsEmoji)
                    {
                        HunterEmbed.Title = hunter.Name + "'s Stats";
                        HunterEmbed.Description = "";

                        int defense = 0;
                        for (int i = 0; i < hunter.ArmorSlots.Length; i++)
                        {
                            defense += hunter.ArmorSlots[i].Defense;
                        }

                        HunterEmbed.AddField("Your total defense is: ", defense.ToString(), true);
                        HunterEmbed.AddField("Your max health is: ", hunter.MaxHealth.ToString(), true);
                        HunterEmbed.AddField("Monsters hunted: ", "0", true); //Track monster kills!!!
                        HunterEmbed.AddField("Your rank is: ", "0", true); // Calculate rank!
                    }
                    else
                    {
                        Armor equipment = null;
                        if (HunterEmbed.Title != hunter.Name + "'s Helmet Slot" && reaction.Result.Emoji == HelmetEmoji)
                        {
                            equipment = hunter.ArmorSlots[0];
                            HunterEmbed.Title = hunter.Name + "'s Helmet Slot";
                        }
                        else if (HunterEmbed.Title != hunter.Name + "'s Chest Slot" && reaction.Result.Emoji == ChestplateEmoji)
                        {
                            equipment = hunter.ArmorSlots[1];
                            HunterEmbed.Title = hunter.Name + "'s Chest Slot";
                        }
                        else if (HunterEmbed.Title != hunter.Name + "'s Bracers Slot" && reaction.Result.Emoji == BracersEmoji)
                        {
                            equipment = hunter.ArmorSlots[2];
                            HunterEmbed.Title = hunter.Name + "'s Bracers Slot";
                        }
                        else if (HunterEmbed.Title != hunter.Name + "'s Waist Slot" && reaction.Result.Emoji == WaistEmoji)
                        {
                            equipment = hunter.ArmorSlots[3];
                            HunterEmbed.Title = hunter.Name + "'s Waist Slot";
                        }
                        else if (HunterEmbed.Title != hunter.Name + "'s Greaves Slot" && reaction.Result.Emoji == GreavesEmoji)
                        {
                            equipment = hunter.ArmorSlots[4];
                            HunterEmbed.Title = hunter.Name + "'s Greaves Slot";
                        }
                        else
                            continue;

                        HunterEmbed.Description = "***" + equipment.Name + "***";
                        HunterEmbed.AddField("Defense", equipment.Defense.ToString());
                        HunterEmbed.AddField("Description", equipment.Description);
                    }

                    await HunterDisplay.ModifyAsync(default, new Optional<DiscordEmbed>(HunterEmbed));
                }
            }
            else
                await ctx.Channel.SendMessageAsync("Yeah so I'm not as mean as the other guy but that doesn't mean I'm going to let you use a hunter command " +
                    "when you don't have a hunter. C'mon man...");
        }

        [Command("Attack")]
        public async Task Attack(CommandContext ctx)
        {
            ulong uuid = ctx.Member.Id;

            if (Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
            {
                Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid).Hunter;
                var Interactivity = ctx.Client.GetInteractivity();

                var AttackEmbed = new DiscordEmbedBuilder
                {
                    Title = hunter.Name + "'s Attack Panel",
                    Description = "React to select an ability/move to use!"
                };
                AttackEmbed.WithFooter("00:00");
                AttackEmbed.WithAuthor(ctx.User.Username, default, ctx.User.GetAvatarUrl(ImageFormat.Png));

                List<DiscordEmoji> NumberEmojis = new List<DiscordEmoji>{
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

                //Adds a numbered field for each move available
                for (int i = 0; i < hunter.CurrentWeapon.MoveSet.Count; i++)
                {
                    AttackEmbed.AddField("**" + (i + 1) + ":**", hunter.CurrentWeapon.MoveSet[i].toString());
                }

                var AttackDisplay = await ctx.Channel.SendMessageAsync(embed: AttackEmbed);

                List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
                //Adds an incrementing emoji for each move available
                for (int i = 0; i <= hunter.CurrentWeapon.MoveSet.Count - 1; i++)
                {
                    await AttackDisplay.CreateReactionAsync(NumberEmojis[i]);
                    UsedEmojis.Add(NumberEmojis[i]);
                }


                while(true)
                {
                    var reaction = await Interactivity.WaitForReactionAsync(
                        x => x.Message == AttackDisplay &&
                        x.User.Id == ctx.Member.Id &&
                        UsedEmojis.Contains(x.Emoji)).ConfigureAwait(false);

                    if(reaction.TimedOut)
                    {
                        await AttackDisplay.DeleteAsync();
                        break;
                    }

                    Moves move = hunter.CurrentWeapon.MoveSet[NumberEmojis.IndexOf(reaction.Result.Emoji)];

                    Bot.ServerActiveMonster[ctx.Guild.Id].Monster.TakeDamage(hunter.CurrentWeapon.Attack(move), ctx);
                    UpdateMonster(ctx, true);

                    if(Bot.ServerActiveMonster[ctx.Guild.Id].Monster.Health == 0)
                    {
                        await AttackDisplay.DeleteAsync();
                        break;
                    }

                    for(int i = move.Cooldown; i >= 0; i--)
                    {
                        AttackEmbed = AttackEmbed.WithFooter(move.ClockFormatOfCooldown(i));
                        await AttackDisplay.ModifyAsync(default, new Optional<DiscordEmbed>(AttackEmbed));
                        if(i > 0)
                            await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    await reaction.Result.Message.DeleteReactionAsync(reaction.Result.Emoji, ctx.User);
                }
            }
            else
                await ctx.Channel.SendMessageAsync("Alright so let me get this straight, you're not a hunter... Yet you want to attack this monster?? " +
                    "Doesn't seem like a smart idea. Let's just 'return;' quick");
            //Haha what did you really come to the code to see if it actually used "return;" to get out of the function? No. I'm an else kinda guy but the joke wouldn't have worked
            //unless I pretended I was using return; so just let this one slide, okay?
            
        }

        [Command("GuildCard"), Description("Shows the Hunter's guild card.")]
        public async Task GuildCard(CommandContext ctx)
        {
            Hunter hunter = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == ctx.Member.Id).Hunter;
            GuildCard guildCard = hunter.GuildCard;
            Color textC = guildCard.TextColor;
            Image background = guildCard.Background;

            await ctx.Channel.SendFileAsync(".\\Images\\StartingBackground.jpg");
        }

        public async Task UpdateChannelAsync(CommandContext ctx, Monster Monster)
        {
            string OriginalChannelName = ctx.Channel.Name;
            string OriginalChannelDescription = ctx.Channel.Topic;

            await ctx.Channel.ModifyAsync(a =>
            {
                a.Name = Monster.Name;
                a.Topic = Monster.Health + "/" + Monster.MaxHealth;
            });

            //When monster health drops to 0 revert channel back to normal
            bool monsterAlive = true;
            while (monsterAlive)
            {
                if (Bot.ServerActiveMonster[ctx.Guild.Id].Monster.Health == 0)
                    monsterAlive = false;
            }

            await ctx.Channel.ModifyAsync(a =>
            {
                a.Name = OriginalChannelName;
                a.Topic = OriginalChannelDescription;
            });
        }

        public async Task UpdateDamageDisplay(CommandContext ctx)
        {
            ulong uuid = ctx.Member.Id;
            ConfigHunterJson hunterJson = Bot.ServerHunterList[ctx.Guild.Id].Find(u => u.Uuid == uuid);
            int h = hunterJson.Hunter.Health;
            int maxH = hunterJson.Hunter.MaxHealth;
            double healthDub = (double)h / maxH;
            int green = (int)Math.Round(healthDub * 2 * 255);
            int red = (int)Math.Round(255 - (healthDub * 2 * 255 - 255));
            string hexGreen = green.ToString("X2");
            string hexRed = red.ToString("X2");
            string hexRoleColor =
                healthDub >= .5 ? hexRed + "FF00" :
                healthDub <= .5 ? "FF" + hexGreen + "00" :
                h == 0 ? "000001" : "FFFF00";
            await hunterJson.Role.ModifyAsync(r => r.Color = new DiscordColor(hexRoleColor));
        }

        public bool NoHunter(CommandContext ctx)
        {
            ulong uuid = ctx.Member.Id;
            if (!Bot.ServerHunterList[ctx.Guild.Id].Any(u => u.Uuid == uuid))
            {
                ctx.Channel.SendMessageAsync("Okay real idiot here with no hunter trying to use a hunter command...");
                return true;
            }
            return false;
        }

        public void UpdateMonster(CommandContext ctx, bool updateChannel = true)
        {
            File.WriteAllText(".\\Servers\\" + ctx.Guild.Id + "\\Monsters\\ActiveMonster.json", JsonConvert.SerializeObject(Bot.ServerActiveMonster[ctx.Guild.Id], Formatting.Indented));
            Monster monster = Bot.ServerActiveMonster[ctx.Guild.Id].Monster;
            
            ctx.Channel.ModifyAsync(a =>
            {
                a.Name = monster.Name;
                a.Topic = monster.Health + "/" + monster.MaxHealth;
            });
        }

        public void UpdateHunterJson(CommandContext ctx)
        {
            File.WriteAllText(".\\Servers\\" + ctx.Guild.Id + "\\Hunters\\" + Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.Id).Uuid + ".json", 
                JsonConvert.SerializeObject(Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.Id), Formatting.Indented));
        }

        public void DeleteHunterJson(CommandContext ctx)
        {
            File.Delete(".\\Servers\\" + ctx.Guild.Id + "\\Hunters\\" + Bot.ServerHunterList[ctx.Guild.Id].Find(h => h.Uuid == ctx.Member.Id).Uuid + ".json");
        }

        public async Task<string> GetUserMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            return (await interactivity.WaitForMessageAsync(u => u.Channel == ctx.Channel && (u.Author == ctx.User || u.Author == ctx.Member))).Result.Content;
        }

        public async Task MonsterAttacks(CommandContext ctx)
        {
            while (Bot.ServerActiveMonster[ctx.Guild.Id].Monster.Health > 0)
            {
                Moves moveused = Bot.ServerActiveMonster[ctx.Guild.Id].Monster.Attack();

                await Task.Delay(TimeSpan.FromSeconds(moveused.Cooldown));
            }
        }

    }
}
