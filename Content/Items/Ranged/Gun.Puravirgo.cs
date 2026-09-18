using Coralite;
using Coralite.Content.CoraliteNotes;
using Coralite.Content.CoraliteNotes.ConstellationChapter;
using Coralite.Content.Items.Misc_Shoot;
using Coralite.Content.Particles;
using Coralite.Core;
using Coralite.Core.Prefabs.Projectiles;
using Coralite.Core.Systems.KeySystem;
using Coralite.Helpers;
using CoraliteExtension.Core;
using InnoVault.GameContent.BaseEntity;
using InnoVault.PRT;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Ranged
{
    public class Puravirgo : ModItem, IConsultableItem
    {
        public override string Texture => AssetDirectoryEX.RangedItems + Name;
        public Knowledge GetKnowledge => CoraliteContent.GetKnowledge<ConstellationKnowledge>();
        public int GetPageIndex => CoraliteNoteUIState.BookPanel.GetPageIndex<ConstellationPage1>();

        public override void SetDefaults()
        {
            Item.SetWeaponValues(85, 7f);
            Item.DefaultToRangedWeapon(ProjectileType<PuravirgoHeldProj>(), AmmoID.Bullet, 24, 13.5f);

            Item.useStyle = ItemUseStyleID.Rapier;
            Item.value = Item.sellPrice(0, 5);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = CoraliteSoundID.Bow2_Item102;

            Item.useTurn = false;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(new EntitySource_ItemUse(player, Item), player.Center, Vector2.Zero, ProjectileType<PuravirgoHeldProj>(), damage, knockback, player.whoAmI);
            Projectile.NewProjectile(source, position, velocity, ProjectileType<PuravirgoFeather>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Virgo>()
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class PuravirgoHeldProj() : BaseGunHeldProj(0.25f, 20, -8, AssetDirectoryEX.RangedItems)
    {
        protected override float HeldPositionY => -6;

        public override void ModifyAI(float factor)
        {
            Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.1f, 0.3f));
        }
    }

    public class PuravirgoFeather : ModProjectile
    {
        public override string Texture => AssetDirectoryEX.RangedItems + Name;

        public Particle chainParticle;

        /// <summary>
        /// 已生成的小弹幕数量
        /// </summary>
        public int spawnedBullets = 0;
        /// <summary>
        /// 最大小弹幕数量
        /// </summary>
        public const int maxBullets = 7;

        public override void SetStaticDefaults()
        {
            Projectile.QuickTrailSets(Helper.TrailingMode.RecordAll, 8);
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 16;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60 * 10 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 在飞行过程中生成小弹幕
            if (Projectile.ai[0] < 28 && (Projectile.ai[0] % 4 == 0) && spawnedBullets < maxBullets)
            {
                float range = Main.rand.NextFloat(0, 1);

                // 生成星云粒子效果
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dir = (i * MathHelper.PiOver2).ToRotationVector2();
                    for (int j = 0; j < 3; j++)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.CrystalPulse, dir * (1 + (j * (0.55f + (0.25f * range))))
                            , Scale: 0.8f + (range * 0.4f) - (j * 0.15f));
                        d.noGravity = true;
                    }
                }

                // 生成星星连线粒子
                var p = PRTLoader.NewParticle<PuravirgoStar>(Projectile.Center,
                    Helper.NextVec2Dir() * Main.rand.NextFloat(0.8f, 1.8f),
                    new Color(255, 35, 170), 0.01f);

                if (chainParticle != null)
                    p.ChainedParticle = chainParticle;

                p.Alpha = 0.85f;
                p.TargetScale = 0.8f;
                p.ShineTime = 3;
                p.FadeTime = 8;
                p.LineWidth = 16;
                chainParticle = p;

                // 生成小弹幕
                Projectile.NewProjectileFromThis<PuravirgoBullet>(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.Zero) * (2.5f + Projectile.ai[0] / 4 * (2.5f / 7)),
                    (int)(Projectile.damage * 0.55f), Projectile.knockBack / 5);

                spawnedBullets++;
            }

            Projectile.ai[0]++;
        }

        public override void OnKill(int timeLeft)
        {
            // 生成剩余未能放出的小弹幕
            int remainingBullets = maxBullets - spawnedBullets;

            for (int i = 0; i < remainingBullets; i++)
            {
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(4f, 6f);
                Projectile.NewProjectileFromThis<PuravirgoBullet>(Projectile.Center, velocity,
                    (int)(Projectile.damage * 0.45f), Projectile.knockBack / 5);
            }

            // 死亡特效
            for (int i = 0; i < 4; i++)
            {
                Vector2 dir2 = (i * MathHelper.PiOver2).ToRotationVector2();
                for (int j = 0; j < 6; j++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, dir2 * (1 + (j * 0.8f)), Scale: 1.6f - (j * 0.15f));
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color c = new Color(255, 100, 255);
            c.A = 50;
            Projectile.DrawShadowTrails(c, 0.7f, 0.7f / 8, 0, 8, 1, 0, Projectile.scale * 0.8f);
            Projectile.QuickDraw(lightColor, 0);
            return false;
        }
    }

    public class PuravirgoBullet : BaseHeldProj
    {
        public override string Texture => AssetDirectoryEX.RangedItems + Name;

        ref float State => ref Projectile.ai[0];
        ref float Target => ref Projectile.ai[1];
        ref float Timer => ref Projectile.ai[2];
        ref float SelfRot => ref Projectile.localAI[0];

        /// <summary>
        /// 反弹次数
        /// </summary>
        public int bounceCount = 0;
        /// <summary>
        /// 最大反弹次数
        /// </summary>
        public const int maxBounces = 6;

        public const int trailCachesLength = 16;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 12;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60 * 10 * Projectile.MaxUpdates;
        }

        public override void Initialize()
        {
            Target = -1;
            Projectile.InitOldPosCache(trailCachesLength);
            Projectile.InitOldRotCache(trailCachesLength);
        }

        public override bool? CanDamage()
        {
            if (State == 0 && Timer < 40)
                return false;

            return null;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.2f, 0.5f));

            SelfRot += Projectile.velocity.Length() / 50;
            switch (State)
            {
                default:
                case 0: // 生成后每隔3帧查找一次目标
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy((Projectile.whoAmI % 2 == 0 ? -1 : 1) * 0.14f);

                        if (Timer > 40)
                        {
                            if (Timer % 8 == 0)
                            {
                                if (Helper.TryFindClosestEnemy(Projectile.Center, 1100, n => n.CanBeChasedBy(), out NPC target))
                                {
                                    Target = target.whoAmI;
                                    State = 2;
                                    Timer = 0;
                                }
                            }
                        }

                        Timer++;

                        if (Timer > 60 * 2 * Projectile.MaxUpdates)
                        {
                            Projectile.Kill();
                        }
                    }
                    break;

                case 2: // 追踪敌人
                    {
                        if (!Target.GetNPCOwner(out NPC target))
                        {
                            State = 3;
                            Timer = 0;
                            break;
                        }

                        if (!target.CanBeChasedBy())
                        {
                            State = 3;
                            Timer = 0;
                            break;
                        }

                        // 使用 ChaseGradually 追踪
                        Projectile.ChaseGradually(target.Center, 16f, 30, 31);
                    }
                    break;

                case 3: // 失去目标，持续飞行120帧后kill
                    {
                        Timer++;
                        if (Timer > 120 * Projectile.MaxUpdates)
                        {
                            Projectile.Kill();
                        }
                    }
                    break;
            }

            Projectile.UpdateOldPosCache(addVelocity: true);
            Projectile.UpdateOldRotCache();

            if (Main.rand.NextBool(3))
                Projectile.SpawnTrailDust(4f, Main.rand.NextFromList(DustID.UnusedWhiteBluePurple, DustID.UnusedWhiteBluePurple, DustID.UnusedWhiteBluePurple, DustID.CrystalPulse, DustID.CrystalPulse2), -Main.rand.NextFloat(0.1f, 0.4f), Scale: Main.rand.NextFloat(0.6f, 1.2f));
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 反弹
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                // 达到最大反弹次数，消失
                Projectile.Kill();
                return true;
            }

            // 使用 TileReflect 进行反弹，速度衰减为0.85倍
            Projectile.TileReflect(oldVelocity, 0.85f);

            // 反弹特效
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, Helper.NextVec2Dir(0.5f, 2f)
                    , Scale: Main.rand.NextFloat(0.6f, 0.9f));
            }

            return false; // 返回false表示不消失
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, Helper.NextVec2Dir(0.5f, 1.5f)
                    , Scale: Main.rand.NextFloat(0.8f, 1f));
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrails();
            Texture2D mainTex = Projectile.GetTextureValue();
            Color c = Color.White;

            var pos = Projectile.Center - Main.screenPosition;
            var origin = mainTex.Size() / 2;

            Main.spriteBatch.Draw(mainTex, pos, null, c, SelfRot, origin, Projectile.scale, 0, 0);
            return false;
        }

        public virtual void DrawTrails()
        {
            if (Projectile.oldPos.Length != trailCachesLength || Projectile.oldRot.Length != trailCachesLength)
                return;

            Texture2D Texture = CoraliteAssets.Trail.CircleSPA.Value;

            CoraliteSystem.InitBars();
            CoraliteSystem.InitBars2();

            List<ColoredVertex> bars = CoraliteSystem.Vertexes;
            List<ColoredVertex> bars2 = CoraliteSystem.Vertexes2;
            Color c = new Color(255, 30, 174) * 0.9f;

            for (int i = 0; i < trailCachesLength; i++)
            {
                float factor = (float)i / trailCachesLength;
                Vector2 Center = Projectile.oldPos[i] - Main.screenPosition;
                Vector2 normal = (Projectile.oldRot[i] + MathHelper.PiOver2).ToRotationVector2();
                Vector2 Top = Center + (normal * 8 * ((factor / 2) + 0.5f));
                Vector2 Bottom = Center - (normal * 8 * ((factor / 2) + 0.5f));

                Vector2 Top2 = Center + (normal * 4);
                Vector2 Bottom2 = Center - (normal * 4);

                var Color = c * factor;

                bars.Add(new(Top, Color, new Vector3(factor, 0, 0)));
                bars.Add(new(Bottom, Color, new Vector3(factor, 1, 0)));
                bars2.Add(new(Top2, Color, new Vector3(factor, 0, 0)));
                bars2.Add(new(Bottom2, Color, new Vector3(factor, 1, 0)));
            }

            if (bars.Count < 2)
                return;

            Main.graphics.GraphicsDevice.Textures[0] = Texture;
            BlendState b = Main.graphics.GraphicsDevice.BlendState;
            Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars2.ToArray(), 0, bars2.Count - 2);
            Main.graphics.GraphicsDevice.BlendState = b;
        }
    }

    public class PuravirgoStar : StarChain
    {
        public override string Texture => AssetDirectoryEX.RangedItems + Name;
    }
}
