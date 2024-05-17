using Coralite.Core;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.CraftConditions;
using Coralite.Core.Systems.ParticleSystem;
using CoraliteExtension.Content.Particles;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CoraliteExtension.Content.Items.MysteryGel
{
    public class MysteryGel : BaseMysteryGelItem, IMagikeRemodelable
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public void AddMagikeRemodelRecipe()
        {
            MagikeSystem.AddRemodelRecipe<MysteryGel>(0f, ItemID.PinkGel, 20, selfStack: 3, condition: HardModeCondition.Instance);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<MysteryGelTile>());
            Item.rare = ModContent.RarityType<MysteryGelRarity>();
            Item.ammo = AmmoID.Gel;
            Item.damage = 4;
            Item.noMelee = true;
        }
    }

    public abstract class BaseMysteryGelItem : ModItem
    {
        private static ParticleGroup group;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            group?.UpdateParticles();
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                group ??= new ParticleGroup();
                if (group != null)
                {
                    if (!Main.gamePaused && Main.GameUpdateCount % 10 == 0)
                    {
                        int type ; 
                        Color c;
                        float speed;
                        float scale;
                        if (Main.rand.NextBool(4))
                        {
                            type = CoraliteContent.ParticleType<GoldSparkle>();
                            c = Color.Pink;
                            speed = Main.rand.NextFloat(0.5f, 0.6f);
                            scale = Main.rand.NextFloat(0.5f, 0.9f);
                        }
                        else
                        {
                            type = CoraliteContent.ParticleType<MysteryGelParticle>();
                            c = Color.White;
                            speed = Main.rand.NextFloat(0.4f, 0.6f);
                            scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }

                        Vector2 size = ChatManager.GetStringSize(line.Font, line.Text, line.BaseScale);
                        group.NewParticle(new Vector2(line.X, line.Y) + new Vector2(Main.rand.NextFloat(0, size.X), size.Y-4),
                            Main.rand.NextFloat(-1.57f - 0.3f, -1.57f + 0.3f).ToRotationVector2() * speed  , type, c, scale);
                    }
                }
                group?.DrawParticlesInUI(Main.spriteBatch);
            }

            return true;
        }
    }

    public class MysteryGelParticle : Particle
    {
        public override string Texture => AssetDirectoryEX.Particles + Name;

        public override void OnSpawn()
        {
            shouldKilledOutScreen = false;
            Frame = new Rectangle(Main.rand.Next(2) * 10, Main.rand.Next(2) * 10, 10, 10);
            Rotation = Main.rand.NextFloat(6.282f);
        }

        public override void Update()
        {
            fadeIn++;

            if (fadeIn % 25 == 0)
                if (Frame.Y < 20)
                    Frame.Y += 10;

            switch (Frame.Y)
            {
                default:
                case 0:
                    Rotation += 0.1f;
                    break;
                case 10:
                    Rotation += 0.2f;
                    if (Main.rand.NextBool(3))
                    {
                        Velocity = Velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
                    }
                    break;
                case 20:
                    Scale *= 0.9f;
                    Velocity *= 0.99f;
                    break;
            }

            if (Scale < 0.001 || fadeIn > 100)
                active = false;
        }
    }

    public class MysteryGelRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float factor = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly)) * 2;
                if (factor < 1)
                    return Color.Lerp(new Color(251, 192, 224), new Color(249, 101, 189), factor);
                return Color.Lerp(new Color(249, 101, 189), new Color(163, 211, 251), (factor - 1));
            }
        }
    }

    public class MysteryGelTile : ModTile
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        private const int sheetWidth = 234;
        private const int sheetHeight = 90;

        public override void SetStaticDefaults()
        {
            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBouncy[Type] = true;

            DustType = DustID.PinkSlime;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = (fail ? 1 : 3);
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int xPos = i % 3;
            int yPos = j % 3;
            frameXOffset = xPos * sheetWidth;
            frameYOffset = yPos * sheetHeight;
        }
    }
}
