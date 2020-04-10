using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public string Uuid { get; set; }
        [JsonProperty("HunterName")]
        public string HunterName { get; set; }
    }
}
