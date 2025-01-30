using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CoraliteExtension.Content.Particles
{
    public class GoldSparkle : Particle
    {
        public override string Texture => AssetDirectoryEX.Particles + Name;

        public override void SetProperty()
        {
            Frame = new Rectangle(0, Main.rand.Next(3) * 22, 22, 22);
            ShouldKillWhenOffScreen = false;
        }

        public override void AI()
        {
            Opacity++;

            if (Opacity % 5 == 0)
            {
                Frame.X += 22;
                if (Frame.X > 3 * 22)
                    Frame.X = 0;
            }

            if (Opacity > 45)
                Color *= 0.9f;

            if (Color.A < 10 || Opacity > 90)
            {
                active = false;
            }
        }

        public override void DrawInUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TexValue, Position, Frame, Color, Rotation, Frame.Size() / 2, Scale, 0, 0);
        }
    }
}
