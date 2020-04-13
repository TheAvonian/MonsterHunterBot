using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus.CommandsNext;

namespace MonsterHunterBot
{
    public class GuildCard
    {
        public Image Background { get; set; } = Image.FromFile(".\\Images\\StartingBackground.jpg");
        public Color TextColor { get; set; } = Color.White;
        public string Path { get; set; }

    }
}
