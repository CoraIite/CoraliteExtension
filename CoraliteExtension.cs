global using Particle = InnoVault.PRT.BasePRT;

using Terraria.ModLoader;
using Coralite.Core;

namespace CoraliteExtension
{
    public class CoraliteExtension : Mod, ICoralite
    {
        public static CoraliteExtension Instance { get; private set; }

        public CoraliteExtension()
        {
            Instance = this;
        }
    }
}