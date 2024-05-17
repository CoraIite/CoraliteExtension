using Coralite.Core.Systems.ParticleSystem;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace CoraliteExtension.Content.Particles
{
    public class GoldSparkle : Particle
    {
        public override string Texture => AssetDirectoryEX.Particles + Name;

        public override void OnSpawn()
        {
            Frame = new Rectangle(0, Main.rand.Next(3) * 22, 22, 22);
            shouldKilledOutScreen = false;
        }

        public override void Update()
        {
            fadeIn++;

            if (fadeIn % 5 == 0)
            {
                Frame.X += 22;
                if (Frame.X > 3 * 22)
                    Frame.X = 0;
            }

            if (fadeIn > 45)
                color *= 0.9f;

            if (color.A < 10 || fadeIn > 90)
            {
                active = false;
            }
        }
    }
}
