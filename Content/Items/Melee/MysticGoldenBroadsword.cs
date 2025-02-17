using Coralite.Content.Items.Gels;
using Coralite.Content.Particles;
using Coralite.Core;
using Coralite.Core.Configs;
using Coralite.Core.Prefabs.Projectiles;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.EnchantSystem;
using Coralite.Core.Systems.MagikeSystem.MagikeCraft;
using Coralite.Helpers;
using CoraliteExtension.Content.Particles;
using CoraliteExtension.Core;
using InnoVault;
using InnoVault.PRT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Coralite.Coralite;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Melee
{
    public class MysticGoldenBroadsword : ModItem, IMagikeCraftable
    {
        public override string Texture => AssetDirectoryEX.MeleeItems + Name;

        private static PRTGroup group;
        private int useCount;

        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.damage = 23;
            Item.useTime = Item.useAnimation = 22;
            Item.knockBack = 2f;

            Item.useStyle = ItemUseStyleID.Rapier;
            Item.DamageType = DamageClass.Melee;
            Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.rare = RarityType<MysticGoldenRarity>();
            Item.shoot = ProjectileType<MysticGoldenBroadswordSlash>();

            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                int combo = 0;
                if (useCount > 1)
                {
                    if (Main.rand.NextBool(useCount, 4))
                    {
                        combo = Main.rand.Next(1, 3);
                        useCount = 0;
                    }
                }

                Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, combo);
            }

            useCount++;
            if (useCount > 3)
                useCount = 0;

            return false;
        }

        public override bool AllowPrefix(int pre)
        {
            return true;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            group?.Update();
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                group ??= new ();
                if (group != null)
                {
                    if (!Main.gamePaused && Main.GameUpdateCount % 20 == 0)
                    {
                        Vector2 size = ChatManager.GetStringSize(line.Font, line.Text, line.BaseScale);
                        group.NewParticle(new Vector2(line.X, line.Y) + new Vector2(Main.rand.NextFloat(0, size.X), Main.rand.NextFloat(0, size.Y)),
                            new Vector2(0, Main.rand.NextFloat(-0.2f, 0.2f)), CoraliteContent.ParticleType<GoldSparkle>()
                            , Main.rand.NextBool() ? Color.White : Color.Gold, Main.rand.NextFloat(0.4f, 0.7f));
                    }
                }
                group?.DrawInUI(Main.spriteBatch);
            }

            return true;
        }

        public void AddMagikeCraftRecipe()
        {
            //MagikeSystem.AddRemodelRecipe( ItemID.GoldBroadsword,ItemType< MysticGoldenBroadsword >(),
            //    MagikeHelper.CalculateMagikeCost(MALevel.Glistent,24,60*8), conditions:Condition.DownedEyeOfCthulhu);
        }
    }

    public class MysticGoldenRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float factor = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly)) * 2;
                if (factor < 1)
                    return Color.Lerp(Color.White, new Color(255, 249, 181), factor);
                return Color.Lerp(new Color(255, 249, 181), new Color(203, 179, 73), (factor - 1));
            }
        }
    }

    public class MysticGoldenBroadswordSlash : BaseSwingProj,IDrawWarp
    {
        public override string Texture => AssetDirectoryEX.MeleeItems + "MysticGoldenBroadsword";

        public ref float Combo => ref Projectile.ai[0];

        public static Asset<Texture2D> WarpTexture;
        public static Asset<Texture2D> GradientTexture;

        public MysticGoldenBroadswordSlash() : base(new Vector2(60, 60).ToRotation(), trailCount: 38) { }

        public int delay;
        public int alpha;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            WarpTexture = Request<Texture2D>(AssetDirectory.OtherProjectiles + "WarpTex");
            GradientTexture = Request<Texture2D>(AssetDirectoryEX.MeleeItems + "MysticGoldenGradient");
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            WarpTexture = null;
            GradientTexture = null;
        }

        public override void SetSwingProperty()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.hide = true;
            Projectile.localNPCHitCooldown = 48;
            Projectile.width = 40;
            Projectile.height = 90;
            trailTopWidth = -6;
            distanceToOwner = 4;
            minTime = 0;
            useSlashTrail = true;
        }

        protected override float ControlTrailBottomWidth(float factor)
        {
            return 70 * Projectile.scale;
        }

        protected override void InitBasicValues()
        {
            if (Main.myPlayer == Projectile.owner)
                Owner.direction = Main.MouseWorld.X > Owner.Center.X ? 1 : -1;

            Projectile.extraUpdates = 3;
            alpha = 0;
            switch (Combo)
            {
                default:
                case 0: //下挥，较为椭圆
                    startAngle = 2.4f + Main.rand.NextFloat(-0.2f, 0.2f);
                    totalAngle = 2.6f + Main.rand.NextFloat(-0.2f, 0.8f);
                    maxTime = (int)(Owner.itemTimeMax * 0.7f) + 38;
                    Smoother = Instance.BezierEaseSmoother;
                    delay = 4;
                    SoundEngine.PlaySound(CoraliteSoundID.Swing_Item1, Projectile.Center);

                    break;
                case 1://下挥，圆
                    startAngle = 2.2f;
                    totalAngle = 4.4f;
                    minTime = 0;
                    maxTime = (int)(Owner.itemTimeMax * 0.7f) + 46;
                    Smoother = Instance.BezierEaseSmoother;
                    delay = 8;
                    Helper.PlayPitched("Misc/Slash", 0.4f, 0.1f, Owner.Center);

                    break;
                case 2://上挥 较圆
                    startAngle = -1.6f;
                    totalAngle = -4.6f;
                    minTime = 0;
                    maxTime = (int)(Owner.itemTimeMax * 0.6f) + 46;
                    Smoother = Instance.BezierEaseSmoother;
                    delay = 8;
                    Helper.PlayPitched("Misc/Slash", 0.4f, 0.1f, Owner.Center);

                    break;
            }
        }

        protected override float GetStartAngle() => Owner.direction > 0 ? 0f : MathHelper.Pi;

        protected override void AIBefore()
        {
            Lighting.AddLight(Projectile.Center, 1f, 1f, 0.3f);
            base.AIBefore();
        }

        protected override void OnSlash()
        {
            int timer = (int)Timer - minTime;

            Vector2 dir = RotateVec2.RotatedBy(1.57f * Math.Sign(totalAngle));

            if (timer % 2 == 0)
            {
                Dust dust = Dust.NewDustPerfect(Top - 12 * RotateVec2 + Main.rand.NextVector2Circular(30, 30), DustID.GoldFlame,
                       dir * Main.rand.NextFloat(0.5f, 2f), Scale: Main.rand.NextFloat(0.8f, 1.1f));
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(12))
            {
                PRTLoader.NewParticle(Top - RotateVec2 * 16 + Main.rand.NextVector2Circular(32, 32), dir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(0.5f, 1.5f),
                    CoraliteContent.ParticleType<HorizontalStar>(), Color.Gold, Main.rand.NextFloat(0.05f, 0.15f));
            }
            switch ((int)Combo)
            {
                default:
                case 0:
                    alpha = (int)(Instance.X2Smoother.Smoother(timer, maxTime - minTime) * 180) + 60;
                    break;
                case 1:
                    alpha = (int)(Instance.SqrtSmoother.Smoother(timer, maxTime - minTime) * 140) + 100;
                    break;
                case 2:
                    alpha = (int)(Instance.SqrtSmoother.Smoother(timer, maxTime - minTime) * 140) + 100;
                    Projectile.scale = Helper.EllipticalEase(1.6f - 4.6f * Smoother.Smoother(timer, maxTime - minTime), 1.4f, 1.2f);
                    break;
            }
            base.OnSlash();
        }

        protected override void AfterSlash()
        {
            if (alpha > 20)
                alpha -= 5;
            Slasher();
            if (Timer > maxTime + delay)
                Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (onHitTimer == 0)
            {
                onHitTimer = 1;
                Owner.immuneTime += 10;
                if (Main.netMode == NetmodeID.Server)
                    return;

                float strength = 1;
                float baseScale = 1;

                if (VisualEffectSystem.HitEffect_ScreenShaking)
                {
                    PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, RotateVec2, strength, 6, 6, 1000);
                    Main.instance.CameraModifiers.Add(modifier);
                }

                Dust dust;
                float offset = Projectile.localAI[1] + Main.rand.NextFloat(0, Projectile.width * Projectile.scale - Projectile.localAI[1]);
                Vector2 pos = Bottom + RotateVec2 * offset;
                if (VisualEffectSystem.HitEffect_Lightning)
                {
                    dust = Dust.NewDustPerfect(pos, DustType<EmperorSabreStrikeDust>(),
                        Scale: Main.rand.NextFloat(baseScale, baseScale * 0.5f));
                    dust.rotation = _Rotation + MathHelper.PiOver2 + Main.rand.NextFloat(-0.2f, 0.2f);

                    //dust = Dust.NewDustPerfect(pos, DustType<EmperorSabreStrikeDust>(),
                    //         Scale: Main.rand.NextFloat(baseScale * 0.1f, baseScale * 0.2f));
                    //float leftOrRight = Main.rand.NextFromList(-0.3f, 0.3f);
                    //dust.rotation = _Rotation + MathHelper.PiOver2 + leftOrRight + Main.rand.NextFloat(-0.2f, 0.2f);
                }

                if (VisualEffectSystem.HitEffect_Dusts)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 dir = RotateVec2.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f));
                        dust = Dust.NewDustPerfect(pos, DustID.ShimmerSpark, dir * Main.rand.NextFloat(2f, 6f), Scale: Main.rand.NextFloat(1.5f, 2f));
                        dust.noGravity = true;
                    }
                }

                if (VisualEffectSystem.HitEffect_SpecialParticles)
                    ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings()
                    {
                        PositionInWorld = pos,
                        MovementVector = RotateVec2 * Main.rand.NextFloat(2f, 4f),
                    });
            }
        }

        public void DrawWarp()
        {
            WarpDrawer(0.75f);
        }

        protected override void DrawSlashTrail()
        {
            RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
            List<VertexPositionColorTexture> bars = new List<VertexPositionColorTexture>();
            GetCurrentTrailCount(out float count);

            for (int i = 0; i < oldRotate.Length; i++)
            {
                if (oldRotate[i] == 100f)
                    continue;

                float factor = 1f - i / count;
                Vector2 Center = GetCenter(i);
                Vector2 Top = Center + oldRotate[i].ToRotationVector2() * (oldLength[i] + trailTopWidth + oldDistanceToOwner[i]);
                Vector2 Bottom = Center + oldRotate[i].ToRotationVector2() * (oldLength[i] - ControlTrailBottomWidth(factor) + oldDistanceToOwner[i]);

                var topColor = Color.Lerp(new Color(238, 218, 130, alpha), new Color(167, 127, 95, 0), 1 - factor);
                var bottomColor = Color.Lerp(new Color(109, 73, 86, alpha), new Color(83, 16, 85, 0), 1 - factor);
                bars.Add(new(Top.Vec3(), topColor, new Vector2(factor, 0)));
                bars.Add(new(Bottom.Vec3(), bottomColor, new Vector2(factor, 1)));
            }

            if (bars.Count > 2)
            {
                Helper.DrawTrail(Main.graphics.GraphicsDevice, () =>
                {
                    Effect effect = Filters.Scene["SimpleGradientTrail"].GetShader().Shader;

                    effect.Parameters["transformMatrix"].SetValue(VaultUtils.GetTransfromMatrix());
                    effect.Parameters["sampleTexture"].SetValue(CoraliteAssets.Trail.LiteSlashBright.Value);
                    effect.Parameters["gradientTexture"].SetValue(GradientTexture.Value);

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes) //应用shader，并绘制顶点
                    {
                        pass.Apply();
                        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
                        Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
                        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
                    }
                }, BlendState.NonPremultiplied, SamplerState.PointWrap, RasterizerState.CullNone);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
            }
        }
    }
}
