using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using MonsterHunterBot.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace MonsterHunterBot
{
    class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public static Dictionary<ulong, List<ConfigHunterJson>> ServerHunterList { get; set; } = new Dictionary<ulong, List<ConfigHunterJson>>();
        public static Dictionary<ulong, ConfigMonsterJson> ServerActiveMonster { get; set; } = new Dictionary<ulong, ConfigMonsterJson>();

        public static SqlConnection DatabaseConnection { get; } = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB;Initial Catalog = MHBotDatabase;Integrated Security = True");
        public static SqlCommand Database { get; set; }
        public async Task RunAsync()
        {
            string json = string.Empty;
            
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding()))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            string[] serverDirectories = Directory.GetDirectories(".\\Servers");
            foreach(string sD in serverDirectories)
            {
                var jsonFiles = Directory.EnumerateFiles(sD + "\\Hunters", "*.json");
                var huntersTemp = new List<ConfigHunterJson>();
                foreach (string j in jsonFiles)
                {
                    using (var fs = File.OpenRead(j))
                    using (var sr = new StreamReader(fs, new UTF8Encoding()))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    huntersTemp.Add(JsonConvert.DeserializeObject<ConfigHunterJson>(json));
                }
                ServerHunterList[ulong.Parse(sD.Substring(sD.LastIndexOf('\\') + 1))] = huntersTemp;

                try
                {
                    using (var fs = File.OpenRead(sD + "\\Monsters\\ActiveMonster.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding()))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    ServerActiveMonster[ulong.Parse(sD.Substring(sD.LastIndexOf('\\') + 1))] = JsonConvert.DeserializeObject<ConfigMonsterJson>(json);
                }
                catch(FileNotFoundException)
                {
                    Console.WriteLine("No Active Monster json in {0}\\Monsters\\ActiveMonster.json", sD);
                }
            }
            
            
            
            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(45)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<BasicCommands>();
            Commands.RegisterCommands<MonsterHunterCommands>();
            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
