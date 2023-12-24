using Coralite.Core.Loaders;
using Coralite.Core.Systems.ParticleSystem;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CoraliteExtension.Content.Particles
{
    public class GoldSparkle : ModParticle
    {
        public override string Texture => AssetDirectoryEX.Particles + Name;

        public override void OnSpawn(Particle particle)
        {
            particle.frame = new Rectangle(0, Main.rand.Next(3) * 22, 22, 22);
            particle.shouldKilledOutScreen = false;
        }

        public override void Update(Particle particle)
        {
            particle.fadeIn++;

            if (particle.fadeIn % 5 == 0)
            {
                particle.frame.X += 22;
                if (particle.frame.X > 3 * 22)
                    particle.frame.X = 0;
            }

            if (particle.fadeIn > 45)
                particle.color *= 0.9f;

            if (particle.color.A < 10 || particle.fadeIn > 90)
            {
                particle.active = false;
            }
        }
    }
}
