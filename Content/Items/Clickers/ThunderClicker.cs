using Coralite.Content.Bosses.ThunderveinDragon;
using Coralite.Content.Items.Thunder;
using Coralite.Core;
using Coralite.Helpers;
using CoraliteExtension.Content.Compats;
using InnoVault;
using InnoVault.PRT;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class ThunderClicker() : BaseClickerWeapon(5.5f, Coralite.Coralite.ThunderveinYellow, DustID.YellowTorch, 51)
    {
        public static string ThunderEffect { get; private set; } = string.Empty;
        public static readonly int DamageRatioPercent = 150;

        public override void SetOtherStaticDefaults()
        {
            string uniqueName = ClickerCompat.RegisterClickEffect(Mod, nameof(ThunderEffect), 10, Coralite.Coralite.ThunderveinYellow,
                delegate (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
                {
                    if (Helper.TryFindClosestEnemy(position, 400, n => n.CanBeChasedBy() && (Collision.CanHit(position, 1, 1, n.Center, 1, 1)||n.getRect().Contains(Main.MouseWorld.ToPoint())), out NPC target))
                    {
                        Projectile.NewProjectile(source, position, target.Center,
                            ProjectileType<ThunderClickerProj>(), (int)(damage * DamageRatioPercent / 100f), 0, Main.myPlayer, 30, target.whoAmI);
                    }
                },
            preHardMode: false,
            descriptionArgs: [DamageRatioPercent]);

            ThunderEffect = uniqueName;
        }

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, $"{nameof(CoraliteExtension)}:{nameof(ThunderEffect)}");

            Item.SetShopValues(Terraria.Enums.ItemRarityColor.Yellow8, Item.sellPrice(0, 2, 50));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ZapCrystal>()
                .AddIngredient<InsulationCortex>(2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ThunderClickerProj : BaseThunderProj
    {
        public override string Texture => AssetDirectory.Blank;

        public ref float DashTime => ref Projectile.ai[0];
        public ref float Target => ref Projectile.ai[1];

        public ref float Timer => ref Projectile.localAI[0];

        const int DelayTime = 30;

        protected ThunderTrail[] thunderTrails;

        public override bool ShouldUpdatePosition() => false;

        public override void SetDefaults()
        {
            ClickerCompat.SetClickerProjectileDefaults(Projectile);
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 14;
        }

        public override bool? CanDamage()
        {
            if (Timer > DashTime + (DelayTime / 2))
                return false;

            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float a = 0;
            return targetHitbox .Intersects(projHitbox)|| Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.velocity, Projectile.Center, Projectile.width, ref a);
        }

        public override float ThunderWidthFunc_Sin(float factor)
        {
            return ThunderWidth;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI != (int)Target)
                return false;
            return base.CanHitNPC(target);
        }

        public override void AI()
        {
            if (!Target.GetNPCOwner(out NPC target, Projectile.Kill))
                return;
            Projectile.velocity = target.Center;

            if (Projectile.IsOwnedByLocalPlayer())
            {
                Projectile.Center = Main.MouseWorld;
                if (Timer < DashTime && (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.velocity, 1, 1) || Vector2.DistanceSquared(Projectile.Center, Projectile.velocity) > 400 * 400))
                {
                    Timer = DashTime;
                }
            }

            Lighting.AddLight(Projectile.Center, Coralite.Coralite.ThunderveinYellow.ToVector3());
            if (thunderTrails == null)
            {
                Projectile.Resize(32, 40);
                thunderTrails = new ThunderTrail[2];
                Asset<Texture2D> trailTex = Request<Texture2D>(AssetDirectory.OtherProjectiles + "LightingBodyF");
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                        thunderTrails[i] = new ThunderTrail(trailTex, ThunderWidthFunc_Sin, ThunderColorFunc2_Orange, GetAlpha);
                    else
                        thunderTrails[i] = new ThunderTrail(trailTex, ThunderWidthFunc_Sin, ThunderColorFunc_Yellow, GetAlpha);
                    thunderTrails[i].UseNonOrAdd = true;
                    thunderTrails[i].CanDraw = false;
                    thunderTrails[i].PartitionPointCount = 3;
                    thunderTrails[i].SetRange((0, 6));
                    thunderTrails[i].SetExpandWidth(2);
                    thunderTrails[i].BasePositions =
                    [
                        Projectile.Center,Projectile.Center,Projectile.Center
                    ];
                }
            }

            if (Timer < DashTime)
            {
                SpawnDusts();
                UpdateTrails();

                ThunderWidth = 14;
                ThunderAlpha = Timer / DashTime;
            }
            else if ((int)Timer == (int)DashTime)
            {
                foreach (var trail in thunderTrails)
                {
                    trail.CanDraw = Main.rand.NextBool();
                    trail.RandomThunder();
                }
            }
            else
            {
                SpawnDusts();
                UpdateTrails();

                float factor = (Timer - DashTime) / DelayTime;
                float sinFactor = MathF.Sin(factor * MathHelper.Pi);
                ThunderWidth = 14 + (sinFactor * 10);
                ThunderAlpha = 1 - Helper.X2Ease(factor);

                foreach (var trail in thunderTrails)
                {
                    trail.SetRange((0, 6 + (sinFactor * 10)));
                    trail.SetExpandWidth((1 - factor) * 6);

                    if (Timer % 6 == 0)
                    {
                        trail.CanDraw = Main.rand.NextBool();
                        trail.RandomThunder();
                    }
                }

                if (Timer > DashTime + DelayTime)
                    Projectile.Kill();
            }

            Timer++;
        }

        public void UpdateTrails()
        {
            Vector2 pos2 = Projectile.velocity;
            List<Vector2> pos =
            [
                Projectile.velocity
            ];

            Vector2 normal = (Projectile.velocity - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(1.57f);

            if (Vector2.Distance(Projectile.velocity, Projectile.Center) < 32)
                pos.Add(Projectile.Center);
            else
                for (int i = 0; i < 40; i++)
                {
                    pos2 = pos2.MoveTowards(Projectile.Center, 32);
                    if (Vector2.Distance(pos2, Projectile.Center) < 32)
                    {
                        pos.Add(Projectile.Center);
                        break;
                    }
                    else
                    {
                        float f1 = ((float)Main.timeForVisualEffects * 0.5f) + (Projectile.whoAmI / 2);
                        float f2 = i * 0.4f;
                        float factor = MathF.Sin(f1 + f2) + MathF.Cos(f2 + (f1 / 2));
                        pos.Add(pos2 + (normal * factor * 22));
                    }
                }

            foreach (var trail in thunderTrails)
                trail.BasePositions = [.. pos];

            if (Timer % 5 == 0)
            {
                foreach (var trail in thunderTrails)
                {
                    trail.CanDraw = Main.rand.NextBool();
                    trail.RandomThunder();
                }
            }
        }

        public void SpawnDusts()
        {
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = Vector2.Lerp(Projectile.velocity, Projectile.Center, Main.rand.NextFloat(0.1f, 0.9f))
                    + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.width / 2);
                if (Main.rand.NextBool())
                    PRTLoader.NewParticle(pos, Vector2.Zero, CoraliteContent.ParticleType<ElectricParticle>(), Scale: Main.rand.NextFloat(0.6f, 1f));
                else
                    Dust.NewDustPerfect(pos, DustType<LightningShineBall>(), Vector2.Zero, newColor: ThunderveinDragon.ThunderveinYellowAlpha, Scale: Main.rand.NextFloat(0.1f, 0.2f));
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X > Owner.Center.X ? 1 : -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (thunderTrails != null)
                foreach (var trail in thunderTrails)
                    trail?.DrawThunder(Main.instance.GraphicsDevice);
            return false;
        }
    }
}
