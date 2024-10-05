using Coralite.Content.Items.FlyingShields;
using Coralite.Content.Particles;
using Coralite.Core;
using Coralite.Core.Configs;
using Coralite.Core.Prefabs.Projectiles;
using Coralite.Core.Systems.FlyingShieldSystem;
using Coralite.Core.Systems.ParticleSystem;
using Coralite.Helpers;
using CoraliteExtension.Content.Items.FlyingShield;
using CoraliteExtension.Content.Items.Melee;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.FlyingShieldPlus
{
    public class CobaltSwordAndShield()
        : BaseShieldPlusWeapon<CobaltShieldPlusGuard>(Item.sellPrice(0, 4), ItemRarityID.LightRed, AssetDirectoryEX.FlyingShieldPlusItems)
    {
        public override int FSProjType => ModContent.ProjectileType<CobaltFlyingShieldPlusProj>();

        private int combo;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            ItemID.Sets.ToolTipDamageMultiplier[Type] = 0.5f;
        }

        public override void SetDefaults2()
        {
            base.SetDefaults2();
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.useTime = Item.useAnimation = 30;
            Item.shoot = ModContent.ProjectileType<CobaltSwordSlash>();
            Item.knockBack = 3.5f;
            Item.shootSpeed = 15;
            Item.damage = 85;

            Item.useTurn = false;
            Item.autoReuse = true;
        }

        public override void LeftAttack(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (combo == 2)
                damage = (int)(damage * 1.35f);

            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, (int)(damage *0.75f), knockback, player.whoAmI, combo);

            combo++;
            if (combo > 2)
                combo = 0;
        }

        public override void ShootFlyingShield(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 velocity, int type, int damage, float knockback)
        {
            damage /= 2;
            base.ShootFlyingShield(player, source, velocity, type, damage, knockback);
        }

        public override void RightShoot(Player player, IEntitySource source, int damage)
        {
            damage = (int)(damage * 0.7f);
            base.RightShoot(player, source, damage);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CobaltShield)
                .AddIngredient(ItemID.CobaltSword)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();

            CreateRecipe()
                .AddIngredient<CobaltFlyingShield>()
                .AddIngredient(ItemID.CobaltSword)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    public class CobaltFlyingShieldPlusProj : BaseFlyingShield
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldPlusItems + "CobaltShieldPlus";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 36;
        }

        public override void SetOtherValues()
        {
            flyingTime = 16;
            backTime = 12;
            backSpeed = 16;
            trailCachesLength = 6;
            trailWidth = 20 / 2;
            maxJump++;
        }

        public override void OnShootDusts()
        {
            extraRotation += 0.6f;
        }

        public override void OnBackDusts()
        {
            extraRotation += 0.6f;
        }

        public override Color GetColor(float factor)
        {
            return Color.CadetBlue * factor;
        }
    }

    public class CobaltShieldPlusGuard : BaseFlyingShieldPlusGuard
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldPlusItems + "CobaltShieldPlus";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 42;
            Projectile.height = 42;
        }

        public override void SetOtherValues()
        {
            scalePercent = 2f;
            distanceAdder = 2;
            delayTime = 10;
        }

        public override float GetWidth()
        {
            return Projectile.width * 0.5f / Projectile.scale;
        }
    }

    public class CobaltSwordSlash : BaseSwingProj, IDrawWarp
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldPlusItems + "CobaltSword";

        public ref float Combo => ref Projectile.ai[0];

        public Vector2 recordCenter;
        public bool channel = true;

        private int recordDirection = 1;
        private float recordStartAngle;
        private float recordTotalAngle;
        private float recordAngle;
        private float extraScaleAngle;
        private float channelTimer;

        public const int ChannelTimeMax = 30 * 4;

        public float distance;
        public float distanceA;
        public int delay;
        public int alpha;

        public static Asset<Texture2D> GradientTexture;

        public CobaltSwordSlash() : base(0.785f, trailCount: 16) { }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            GradientTexture = ModContent.Request<Texture2D>(AssetDirectoryEX.FlyingShieldPlusItems + "CobaltSwordAndShieldGradient");
        }

        public override void Unload()
        {
            GradientTexture = null;
        }

        public override void SetDefs()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.localNPCHitCooldown = -1;
            Projectile.width = 40;
            Projectile.height = 80;
            trailTopWidth = 5;
            distanceToOwner = 8;
            onHitFreeze = 6;
            useSlashTrail = true;
            Projectile.hide = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Combo > 2)
            {
                return targetHitbox.Intersects(Utils.CenteredRectangle(OwnerCenter() + recordAngle.ToRotationVector2() * Projectile.height / 2, new Vector2(Projectile.height * 1.5f)));
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        protected override float ControlTrailBottomWidth(float factor)
        {
            if (Combo>2)
            {
                return 75 * Projectile.scale;
            }
            return 90 * Projectile.scale;
        }

        protected override void Initializer()
        {
            if (Combo < 3 && Main.myPlayer == Projectile.owner)
                Owner.direction = Main.MouseWorld.X > Owner.Center.X ? 1 : -1;

            recordCenter = Owner.Center;
            Projectile.extraUpdates = 4;
            alpha = 0;
            recordDirection = OwnerDirection;
            switch (Combo)
            {
                default:
                case 0: //下挥
                case 3:
                case 4:
                    startAngle = 2.2f + Main.rand.NextFloat(-0.2f, 0.2f);
                    totalAngle = 4.6f + Main.rand.NextFloat(-0.2f, 0.2f);
                    minTime = 4;
                    maxTime = minTime + (int)(Owner.itemTimeMax * 0.7f) + 34;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;
                    delay = 8;
                    ExtraInit();
                    Projectile.scale = Helper.EllipticalEase(recordStartAngle + extraScaleAngle - recordTotalAngle * Smoother.Smoother(0, maxTime - minTime), 1f, 1.2f);

                    break;
                case 1://下挥，圆
                    startAngle = 1.8f + Main.rand.NextFloat(-0.2f, 0.2f);
                    totalAngle = 3.4f + Main.rand.NextFloat(-0.2f, 0.2f);
                    minTime = 4;
                    maxTime = minTime + (int)(Owner.itemTimeMax * 0.7f) + 34;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;
                    delay = 8;
                    ExtraInit();
                    Projectile.scale = Helper.EllipticalEase(recordStartAngle + extraScaleAngle - recordTotalAngle * Smoother.Smoother(0, maxTime - minTime), 1.2f, 1.4f);

                    break;
                case 2:
                case 5:
                    startAngle = 2f + Main.rand.NextFloat(-0.2f, 0.2f);
                    totalAngle = 4.7f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;
                    maxTime = minTime + (int)(Owner.itemTimeMax * 0.7f) + 44;
                    minTime = 10;
                    delay = 24;

                    ExtraInit();
                    extraScaleAngle = Main.rand.NextFloat(-0.2f, 0.2f);
                    Projectile.scale = Helper.EllipticalEase(recordStartAngle + extraScaleAngle - recordTotalAngle * Smoother.Smoother(0, maxTime - minTime), 1.2f, 1.7f);

                    break;
            }

            if (Combo > 2)
            {
                onHitFreeze = 0;

                Projectile.scale = Helper.EllipticalEase(recordStartAngle + extraScaleAngle - recordTotalAngle * Smoother.Smoother(0, maxTime - minTime), 1.2f, 1.6f);
                extraScaleAngle = 0;

                trailCount = 48;
                recordAngle = GetStartAngle();
                Projectile.extraUpdates = maxTime;
                totalAngle -= 0.4f;
                Projectile.height = 70;

                useSlashTrail = true;
                Projectile.hide = false;
                delay += 55;
                base.Initializer();
                return;
            }

            Projectile.velocity *= 0f;
            if (Owner.whoAmI == Main.myPlayer)
            {
                _Rotation = GetStartAngle() - OwnerDirection * startAngle;//设定起始角度
            }

            Slasher();
            Smoother.ReCalculate(maxTime - minTime);

            if (useShadowTrail || useSlashTrail)
            {
                oldRotate = new float[trailCount];
                oldDistanceToOwner = new float[trailCount];
                oldLength = new float[trailCount];
                InitializeCaches();
            }

            onStart = false;
            Projectile.netUpdate = true;
        }

        private void ExtraInit()
        {
            extraScaleAngle = Main.rand.Next(-2, 3) * 0.25f;
            recordStartAngle = Math.Abs(startAngle);
            recordTotalAngle = Math.Abs(totalAngle);
        }

        protected override void AIBefore()
        {
            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 1f);
            if (Combo < 3)
                base.AIBefore();
        }

        protected override void BeforeSlash()
        {
            if (Combo > 2)
                return;

            recordDirection = OwnerDirection;
            if (Main.mouseLeft && channel)
            {
                Timer = 1;
                _Rotation = GetStartAngle() - OwnerDirection * startAngle;
                Slasher();
                if (channelTimer < ChannelTimeMax)
                {
                    channelTimer++;
                    startAngle += 0.5f / ChannelTimeMax;
                    return;
                }

                channel = false;
                delay += 80;
                maxTime += 40;
                Projectile.damage *= 2;

                var st = CoraliteSoundID.Slash_Item71;
                st.Pitch = 0.2f;
                SoundEngine.PlaySound(st, Projectile.Center);
                SoundEngine.PlaySound(CoraliteSoundID.TerraBlade_Item60, Projectile.Center);
                return;
            }

            Timer = minTime + 1;
            recordAngle = GetStartAngle();
            _Rotation = startAngle = recordAngle - OwnerDirection * startAngle;//设定起始角度
            totalAngle *= OwnerDirection;
            if (channelTimer >= ChannelTimeMax)
            {
                Smoother = Coralite.Coralite.Instance.HeavySmootherInstance;

                //射弹幕
                if (!channel)
                    Projectile.NewProjectileFromThis<CobaltSwordSlash>(Owner.Center, Vector2.Zero
                        , Projectile.damage , Projectile.knockBack, Combo + 3);
            }
        }

        protected override void OnSlash()
        {
            int timer = (int)Timer - minTime;
            float scale = 1f;

            if (Combo > 2 && Timer == maxTime)
            {
                Projectile.extraUpdates = 4;
                distanceA = 9;
            }

            if (alpha < 255)
                alpha += 8;

            if (Owner.HeldItem.type == ModContent.ItemType<CobaltSwordAndShield>())
                scale = Owner.GetAdjustedItemScale(Owner.HeldItem);
            else
                Projectile.Kill();

            float angle = recordStartAngle + extraScaleAngle - recordTotalAngle * Smoother.Smoother(timer, maxTime - minTime);

            Projectile.scale = Combo switch
            {
                0 => scale * Helper.EllipticalEase(angle, 1f, 1.2f),
                1 => scale * Helper.EllipticalEase(angle, 1.2f, 1.4f),
                _ => scale * Helper.EllipticalEase(angle, 1.2f, 1.6f),
            };

            base.OnSlash();
        }

        protected override void AfterSlash()
        {
            distanceA *= 0.96f;
            distance += distanceA;

            if (Combo < 3)
            {
                if (Main.mouseRight)
                {
                    Projectile.Kill();
                    Owner.itemAnimation = Owner.itemTime = 0;
                    return;
                }
                if (alpha > 20)
                    alpha -= 10;
            }
            else
            {
                Vector2 dir = recordAngle.ToRotationVector2();
                if (Timer % 4 == 0)
                {
                    Dust d = Dust.NewDustPerfect(OwnerCenter() + Main.rand.NextVector2Circular(Projectile.height * 1.4f, Projectile.height * 1.4f), DustID.AncientLight
                         , -dir * Main.rand.NextFloat(8, 10));
                    d.noGravity = true;
                }

                if (Timer > maxTime + delay / 2)
                {
                    if (alpha > 10)
                        alpha -= 8;
                }
                else
                {
                    if (Timer % 6 == 0)
                    {
                        Particle.NewParticle<SpeedLine>(OwnerCenter() + Main.rand.NextVector2Circular(Projectile.height * 1.5f, Projectile.height * 1.5f)
                             , dir * Main.rand.NextFloat(6, 8), Color.CadetBlue, Main.rand.NextFloat(0.2f, 0.4f));
                    }
                }
            }

            Slasher();
            if (Timer > maxTime + delay || (Main.mouseRight && Combo < 3))
                Projectile.Kill();
        }

        protected override void AIAfter()
        {
            Owner.direction = recordDirection;
            if (Combo < 3)
                base.AIAfter();
            else
            {
                Top = Projectile.Center + RotateVec2 * (Projectile.scale * Projectile.height / 2 + trailTopWidth);
                Bottom = Projectile.Center - RotateVec2 * (Projectile.scale * Projectile.height / 2);//弹幕的底端和顶端计算，用于检测碰撞以及绘制

                if ((useShadowTrail || useSlashTrail) && Timer < maxTime)
                    UpdateCaches();
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Timer < minTime || Timer > maxTime && Combo < 3)
                return false;

            if (target.noTileCollide || target.friendly || Projectile.hostile)
                return null;

            if (Collision.CanHit(Owner, target))
                return null;

            return false;
        }

        protected override void OnHitEvent(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.85f);

            if (onHitTimer != 1||!VisualEffectSystem.HitEffect_SpecialParticles)
                return;
            
            float offset = Projectile.localAI[1] + Main.rand.NextFloat(0, Projectile.width * Projectile.scale - Projectile.localAI[1]);
            Vector2 pos = Combo < 3 ? Bottom + RotateVec2 * offset : Vector2.Lerp(target.Center, OwnerCenter(), 0.4f);

            float rotation = (target.Center - Projectile.Center).ToRotation();
            float rot;

            Color c = new Color(0, 100, 255, 150);
            for (int i = 0; i < 2; i++)
            {
                rot = Main.rand.NextFloat(6.282f);
                LightShotParticle.Spawn(pos, c, rot + rotation
                    , new Vector2(Main.rand.NextFloat(0.2f, 0.3f) * Helper.EllipticalEase(rot, 1, 0.4f)
                    , 0.02f));
                rot = Main.rand.NextFloat(6.282f);
                LightShotParticle.Spawn(pos, Color.SkyBlue, rot + rotation
                    , new Vector2(Main.rand.NextFloat(0.3f, 0.4f) * Helper.EllipticalEase(rot, 1, 0.4f)
                    , 0.01f));
            }

            for (int i = 0; i < 3; i++)
                LightTrailParticle_NoPrimitive.Spawn(pos, Helper.NextVec2Dir(3f, 4f), c, Main.rand.NextFloat(0.1f, 0.15f));
        }

        protected override float GetExRot()
        {
            if (Combo < 3 && Timer <= minTime)
            {
                int dir = recordDirection;
                float extraRot = OwnerDirection < 0 ? MathHelper.Pi : 0;
                extraRot += OwnerDirection == dir ? 0 : MathHelper.Pi;
                extraRot += spriteRotation * dir;

                return extraRot;
            }
            return base.GetExRot();
        }

        protected override SpriteEffects CheckEffect()
        {
            if (Combo < 3 && Timer <= minTime)
            {
                if (OwnerDirection < 0)
                {
                    if (recordDirection > 0)
                        return SpriteEffects.None;
                    return SpriteEffects.FlipHorizontally;
                }

                if (recordDirection > 0)
                    return SpriteEffects.None;
                return SpriteEffects.FlipHorizontally;
            }

            return base.CheckEffect();
        }

        protected override void DrawSelf(Texture2D mainTex, Vector2 origin, Color lightColor, float extraRot)
        {
            if (Combo > 2)
                return;

            base.DrawSelf(mainTex, origin, lightColor, extraRot);
        }

        public void DrawWarp()
        {
            if (oldRotate != null && Timer > minTime)
                WarpDrawer(0.75f, alpha / 255f);
        }

        protected override Vector2 OwnerCenter()
        {
            if (Combo > 2 && Timer > minTime)
            {
                return recordCenter + recordAngle.ToRotationVector2() * distance;
            }

            return base.OwnerCenter();
        }

        protected override void DrawSlashTrail()
        {
            if (oldRotate == null || Timer <= minTime)
                return;

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
                    Effect effect =  Filters.Scene[Combo>2? "SimpleGradientTrail" : "NoHLGradientTrail"].GetShader().Shader;

                    effect.Parameters["transformMatrix"].SetValue(Helper.GetTransfromMaxrix());
                    effect.Parameters["sampleTexture"].SetValue(Combo > 2 ? CoraliteAssets.Trail.LiteSlashBrightHMirror.Value : CoraliteAssets.Trail.LiteSlashBright.Value);
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
