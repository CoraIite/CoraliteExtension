using Coralite.Helpers;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace CoraliteExtension.Helpers
{
    public static class HelperEX
    {
        public static SlotId PlayPitched(string path, float volume, float pitch, Vector2? position = null)
        {
            if (Main.netMode == NetmodeID.Server)
                return SlotId.Invalid;

            var style = new SoundStyle($"{nameof(CoraliteExtension)}/Sounds/{path}")
            {
                Volume = volume,
                Pitch = pitch,
                MaxInstances = 0
            };

            return SoundEngine.PlaySound(style, position);
        }
    }
}
