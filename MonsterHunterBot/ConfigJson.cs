using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MonsterHunterBot
{
    public struct ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; private set; }
        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }
    }

    public struct ConfigHunterJson
    {
        [JsonProperty("Uuid")]
        public ulong Uuid { get; set; }
        [JsonProperty("Hunter")]
        public Hunter Hunter { get; set; }
        [JsonProperty("Role")]
        public DiscordRole Role { get; set; }
    }

    public struct ConfigMonsterJson
    {
        [JsonProperty("Monster")]
        public Monster Monster { get; set; }
    }
}
