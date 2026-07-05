global using Particle = InnoVault.PRT.BasePRT;
global using Terraria;
global using Microsoft.Xna.Framework;

using Coralite;
using CoraliteExtension.Core;
using Terraria.ModLoader;

namespace CoraliteExtension
{
    public class CoraliteExtension : Mod, ICoralite
    {
        public static CoraliteExtension Instance { get; private set; }

        public string DataPath => AssetDirectoryEX.Datas;

        public CoraliteExtension()
        {
            Instance = this;
        }
    }
}