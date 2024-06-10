using CoraliteExtension.Content.Items.FlyingShield;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.ModPlayers
{
    public class CoraliteEXPlayer : ModPlayer
    {
        public HashSet<string> Effects = new HashSet<string>();

        public override void ResetEffects()
        {
            Effects ??= new HashSet<string>();
            Effects.Clear();
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (HasEffect(nameof(CthulhuFlyingShield)))
                drawInfo.drawPlayer.shield = 5;
            if (HasEffect(nameof(CobaltFlyingShield)))
                drawInfo.drawPlayer.shield = 1;
        }


        /// <summary>
        /// 玩家是否有某个效果，建议使用<see cref="nameof"/>来获取字符串
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns></returns>
        public bool HasEffect(string effectName) => Effects.Contains(effectName);

        /// <summary>
        /// 为玩家添加某个效果，建议使用<see cref="nameof"/>来获取字符串
        /// </summary>
        /// <param name="effectName"></param>
        /// <returns></returns>
        public bool AddEffect(string effectName) => Effects.Add(effectName);
    }
}
