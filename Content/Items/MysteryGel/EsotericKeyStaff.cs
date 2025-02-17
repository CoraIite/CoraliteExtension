using Coralite.Content.Dusts;
using Coralite.Core;
using Coralite.Core.Configs;
using Coralite.Core.Prefabs.Projectiles;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.MagikeCraft;
using Coralite.Helpers;
using CoraliteExtension.Core;
using CoraliteExtension.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.MysteryGel
{
    public class EsotericKeyStaff : BaseMysteryGelItem, IMagikeCraftable
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public override void SetDefaults()
        {
            Item.width = Item.height = 40;
            Item.damage = 54;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.reuseDelay = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 15;
            Item.knockBack = 3;

            Item.value = Item.sellPrice(0, 3, 0, 0);
            Item.rare = RarityType<MysteryGelRarity>();
            Item.shoot = ProjectileType<EsotericGuardianController>();
            Item.UseSound = CoraliteSoundID.FireBall_Item45;

            Item.useTurn = false;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                var projectile = Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer);
                projectile.originalDamage = Item.damage;
                if (player.TryGetModPlayer(out EsotericGuardianPlayer egp))
                    egp.EsotericGuardianMaxOriginDamage = Item.damage;

                player.AddBuff(BuffType<EsotericGuardianBuff>(), 300);
            }
            return false;
        }

        public void AddMagikeCraftRecipe()
        {
            MagikeRecipe.CreateCraftRecipe<MysteryGel,EsotericKeyStaff>(MagikeHelper.CalculateMagikeCost(MALevel.Soul,12,10*60),10)
                .AddIngredient(ItemID.SoulofFright)
                .AddIngredient(ItemID.SoulofMight)
                .AddIngredient(ItemID.SoulofSight)
                .AddIngredient(ItemID.TinPlating, 36)
                .Register();
        }
    }

    public class EsotericGuardianPlayer : ModPlayer
    {
        public int EsotericGuardianMaxOriginDamage;
        public bool esotericGuardian;

        public override void ResetEffects()
        {
            esotericGuardian = false;
        }

        public override void OnRespawn()
        {
            EsotericGuardianMaxOriginDamage = 0;
        }
    }

    public class EsotericGuardianBuff : ModBuff
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.TryGetModPlayer(out EsotericGuardianPlayer egp))
            {
                if (player.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()] > 0)
                {
                    egp.esotericGuardian = true;
                    player.buffTime[buffIndex] = 100;
                }
                else
                {
                    egp.esotericGuardian = false;
                    player.DelBuff(buffIndex);
                    buffIndex--;
                }
            }

            if (player.whoAmI == Main.myPlayer)
            {
                int type = ProjectileType<EsotericGuardian>();
                if (player.ownedProjectileCounts[type] < 3 && player.ownedProjectileCounts[type] > 0)
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        Projectile projectile = Main.projectile[j];
                        if (projectile.active && projectile.owner == player.whoAmI && projectile.type == type)
                            projectile.Kill();
                    }
                }
                else if (player.ownedProjectileCounts[type] < 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero
                            , type, 0, 0f, player.whoAmI, i);
                    }
                }
            }
        }
    }

    public class EsotericGuardianController : ModProjectile
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + "EsotericGuardianCore";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.timeLeft = 5;
            Projectile.minionSlots = 1;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.minion = true;

            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player p = Main.player[Projectile.owner];
            Projectile.position = p.position;

            if (p.TryGetModPlayer(out EsotericGuardianPlayer egp))
            {
                if (egp.esotericGuardian && p.active && !p.dead)
                    Projectile.timeLeft = 2;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
        public override bool? CanDamage() => false;
        public override bool MinionContactDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }

    /// <summary>
    /// 使用ai0传入类型
    /// </summary>
    public class EsotericGuardian : ModProjectile
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public ref float GuardianType => ref Projectile.ai[0];
        public ref float State => ref Projectile.ai[1];
        public ref float Target => ref Projectile.ai[2];

        public ref float Timer => ref Projectile.localAI[0];
        public ref float SonState => ref Projectile.localAI[1];

        private Player Owner => Main.player[Projectile.owner];

        private Vector2 eyePosOffset;
        private float eyeRot;
        private float ShadowOffset;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;

            //ProjectileID.Sets.TrailingMode[Type] = 2;
            //ProjectileID.Sets.TrailCacheLength[Type] = 7;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.timeLeft = 5;
            Projectile.minionSlots = 0;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.minion = true;

            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool MinionContactDamage() => false;

        public enum AIStates
        {
            idle,
            back,
            slash,
            shoot,
            magic,
        }

        public enum GuardianTypes
        {
            melee,
            shoot,
            magic,
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!CheckActive(player))
                return;

            if (player.active && Vector2.Distance(player.Center, Projectile.Center) > 2000f)
            {
                Timer = 0f;
                Target = 0f;
                Projectile.netUpdate = true;
            }

            Projectile.rotation = Projectile.velocity.X / 50;
            if (player.TryGetModPlayer(out EsotericGuardianPlayer egp))
            {
                Projectile.originalDamage = egp.EsotericGuardianMaxOriginDamage;
            }

            if (State == (int)AIStates.idle)
            {
                CircleMovement(44, 250, 0.4f, 0.9f);
                Projectile.spriteDirection = Owner.direction;

                eyePosOffset = Projectile.velocity / 3;
                eyeRot += 0.02f + Projectile.velocity.Length() / 50;

                if (ShadowOffset == 0f && Main.rand.NextBool(80))
                    ShadowOffset = 1;

                if (ShadowOffset > 0)
                {
                    ShadowOffset -= 0.05f;
                    if (ShadowOffset <= 0)
                        ShadowOffset = 0;
                }

                if (Main.rand.NextBool(20))
                {
                    int target = AI_156_TryAttackingNPCs(Projectile);
                    if (target != -1)
                    {
                        Target = target;
                        Projectile.netUpdate = true;
                        ResetState();
                        return;
                    }
                }

                return;
            }
            else if (State == (int)AIStates.back)
            {
                //AI_GetMyGroupIndexAndFillBlackList(Projectile, out var index, out var totalIndexesInGroup);
                Projectile.spriteDirection = Projectile.direction;
                eyePosOffset = Projectile.velocity / 5;
                eyeRot += 0.02f + Projectile.velocity.Length() / 80;

                Vector2 idleSpot = CircleMovement(28, 800f, 0.2f, 0.2f);
                if (Projectile.Distance(idleSpot) < 40)
                {
                    Timer = 0f;
                    State = (int)AIStates.idle;
                    Projectile.netUpdate = true;
                }

                return;
            }

            int targetIndex = (int)Target;
            if (!Main.npc.IndexInRange(targetIndex))
            {
                State = (int)AIStates.back;
                Projectile.netUpdate = true;
                return;
            }

            NPC nPC = Main.npc[targetIndex];
            if (!nPC.CanBeChasedBy(this))
            {
                State = (int)AIStates.back;
                Projectile.netUpdate = true;
                return;
            }

            float distanceX = nPC.Center.X - Projectile.Center.X;
            Projectile.direction = Projectile.spriteDirection = distanceX > 0 ? 1 : -1;
            int directionY = nPC.Center.Y > Projectile.Center.Y ? 1 : -1;

            bool canhit = Collision.CanHitLine(Projectile.Center, 0, 0, nPC.Center, 0, 0);

            switch (State)
            {
                case (int)AIStates.slash:
                    {
                        //先挪到目标面前，然后持续跟踪目标
                        //与目标位置接近时生成斩击弹幕，之后开始转圈圈砍
                        //最后寻找敌人，没找到就返回
                        switch (SonState)
                        {
                            default:
                            case 0:
                                {
                                    Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 12f, 0.45f, 0.65f, 0.97f);

                                    //控制Y方向的移动
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 50)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 8f, 0.35f, 0.55f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    if (Vector2.Distance(Projectile.Center, nPC.Center) < 100)
                                    {
                                        ShadowOffset = 1;
                                        SonState++;
                                    }

                                    eyePosOffset = Projectile.velocity;
                                    eyeRot += 0.1f;
                                }
                                break;
                            case 1:
                                {
                                    if (Timer == 1)
                                    {
                                        SoundEngine.PlaySound(CoraliteSoundID.LaserGun_Item158, Projectile.Center);
                                        //生成斩击弹幕
                                        int combo = Main.rand.Next(3);
                                        float startAngle = (nPC.Center - Projectile.Center).ToRotation() + combo switch
                                        {
                                            0 => -2f,
                                            1 => 2f,
                                            2 => -1f,
                                            _ => 1f,
                                        };

                                        int damage = Projectile.damage + player.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()] * 5;
                                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                                            ProjectileType<EsotericGuardianSlash>(), damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, startAngle, combo);
                                    }

                                    if (Math.Abs(distanceX) > 130)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 9f, 0.35f, 0.85f, 0.97f);
                                    else if (Math.Abs(distanceX) < 70)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.35f, 0.85f, 0.97f);
                                    else
                                        Projectile.velocity.X *= 0.84f;
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 70)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 8f, 0.35f, 0.65f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.84f;

                                    eyePosOffset = (Timer * 0.2f).ToRotationVector2() * 4;
                                    eyeRot += 0.2f;

                                    Dust d = Dust.NewDustPerfect(Projectile.Center + eyePosOffset - new Vector2(4, 4), DustID.PinkSlime, Vector2.Zero, Alpha: 150);
                                    d.noGravity = true;

                                    if (ShadowOffset > 0)
                                    {
                                        ShadowOffset -= 0.05f;
                                        if (ShadowOffset <= 0)
                                            ShadowOffset = 0;
                                    }

                                    if (Timer > 45)
                                    {
                                        ShadowOffset = 0;
                                        Timer = 0;
                                        SonState++;
                                    }

                                    Timer++;
                                }

                                break;
                            case 2:
                                {
                                    if (Math.Abs(distanceX) < 200)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                    else
                                        Projectile.velocity.X *= 0.95f;
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 100)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else if (yLength < 40)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, -directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    eyePosOffset = Vector2.Lerp(eyePosOffset, Projectile.velocity, 0.05f);
                                    eyeRot -= 0.1f;

                                    if (Timer > 20)
                                    {
                                        SonState++;
                                    }
                                    Timer++;
                                }
                                break;
                            case 3:
                                ResearchEnemy();
                                break;
                        }
                    }
                    break;
                case (int)AIStates.shoot:
                    {
                        //先靠近目标，之后在目标周围环游
                        //间隔固定时间射出凝胶弹
                        switch (SonState)
                        {
                            default:
                            case 0:
                                {
                                    Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 10f, 0.3f, 0.45f, 0.97f);

                                    //控制Y方向的移动
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 200)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 7f, 0.25f, 0.35f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    if (Vector2.Distance(Projectile.Center, nPC.Center) < 500)
                                    {
                                        ShadowOffset = 1;
                                        SonState++;
                                    }

                                    eyePosOffset = Projectile.velocity;
                                    eyeRot += 0.1f;
                                }
                                break;
                            case 1:
                                {
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);

                                    if (!canhit)
                                    {
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 12f, 0.2f, 0.35f, 0.97f);
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 8f, 0.18f, 0.25f, 0.97f);
                                    }
                                    else
                                    {
                                        if (Math.Abs(distanceX) > 500)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                        else if (Math.Abs(distanceX) < 400)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                        else
                                            Projectile.velocity.X *= 0.9f;
                                        if (yLength > 150)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 6f, 0.18f, 0.25f, 0.97f);
                                        else
                                            Projectile.velocity.Y *= 0.9f;
                                    }

                                    int subTime = 15 - player.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()] * 2;
                                    if (subTime < 6)
                                        subTime = 6;

                                    if (Timer < 60 && Timer % subTime == 0)
                                    {
                                        SoundStyle st = CoraliteSoundID.QueenSlime2_Bubble_Item155;
                                        st.Volume -= 0.4f;
                                        SoundEngine.PlaySound(st, Projectile.Center);
                                        int damage = Projectile.damage + player.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()] * 2;
                                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center,
                                            (nPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f)) * 9,
                                            ProjectileType<EsotericGuardianBullet>(), damage, Projectile.knockBack, Projectile.owner);
                                    }

                                    eyePosOffset = new Vector2(MathF.Sin(Timer * 0.2f) * 6, MathF.Cos(Timer * 0.1f) * 4);
                                    eyeRot += 0.2f;

                                    if (ShadowOffset > 0)
                                    {
                                        ShadowOffset -= 0.05f;
                                        if (ShadowOffset <= 0)
                                            ShadowOffset = 0;
                                    }

                                    if (Timer > 85)
                                    {
                                        ShadowOffset = 0;
                                        Timer = 0;
                                        SonState++;
                                    }

                                    Timer++;
                                }
                                break;
                            case 2:
                                {
                                    if (Math.Abs(distanceX) > 400)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                    else if (Math.Abs(distanceX) < 50)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                    else
                                        Projectile.velocity.X *= 0.95f;
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 100)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else if (yLength < 40)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, -directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    eyePosOffset = Vector2.Lerp(eyePosOffset, Projectile.velocity, 0.05f);
                                    eyeRot -= 0.1f;

                                    if (Timer > 20)
                                    {
                                        SonState++;
                                    }
                                    Timer++;
                                }
                                break;
                            case 3:
                                ResearchEnemy();
                                break;
                        }
                    }
                    break;
                case (int)AIStates.magic:
                    {
                        //靠近目标后保持不动,之后跟随
                        //之后在周围生成凝胶球，转圈圈射出
                        switch (SonState)
                        {
                            default:
                            case 0:
                                {
                                    Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 10f, 0.3f, 0.45f, 0.97f);

                                    //控制Y方向的移动
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 250)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 7f, 0.25f, 0.35f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    if (Vector2.Distance(Projectile.Center, nPC.Center) < 500)
                                    {
                                        ShadowOffset = 1;
                                        Projectile.velocity *= 0;
                                        SonState++;
                                    }

                                    eyePosOffset = Projectile.velocity;
                                    eyeRot += 0.1f;
                                }
                                break;
                            case 1:
                                {
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);

                                    if (!canhit)
                                    {
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 12f, 0.2f, 0.35f, 0.97f);
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 8f, 0.18f, 0.25f, 0.97f);
                                    }
                                    else
                                    {
                                        if (Math.Abs(distanceX) > 350)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                        else if (Math.Abs(distanceX) < 450)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                        else
                                            Projectile.velocity.X *= 0.9f;
                                        if (yLength > 250)
                                            Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 6f, 0.18f, 0.25f, 0.97f);
                                        else
                                            Projectile.velocity.Y *= 0.9f;
                                    }

                                    eyePosOffset = (Timer * 0.3f).ToRotationVector2() * 6;
                                    eyeRot += 0.25f;

                                    if (ShadowOffset > 0)
                                    {
                                        ShadowOffset -= 0.05f;
                                        if (ShadowOffset <= 0)
                                            ShadowOffset = 0;
                                    }

                                    if (Timer == 1)
                                    {
                                        SoundEngine.PlaySound(CoraliteSoundID.BubbleGun_Item85, Projectile.Center);
                                        int howMany = player.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()];
                                        int shootCount = 3 + howMany / 2;
                                        int damage = Projectile.damage + howMany * 3;

                                        Vector2 dir = (nPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                                        for (int i = 0; i < shootCount; i++)
                                        {
                                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, dir * 8, ProjectileType<EsotericGuardianMagicBall>(),
                                                 damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI, shootCount, i);
                                        }
                                    }

                                    if (Timer > 55)
                                    {
                                        ShadowOffset = 0;
                                        Timer = 0;
                                        SonState++;
                                    }

                                    Timer++;
                                }
                                break;
                            case 2:
                                {
                                    if (Math.Abs(distanceX) > 80)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                    else if (Math.Abs(distanceX) < 50)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.X, -Projectile.direction, 9f, 0.2f, 0.35f, 0.97f);
                                    else
                                        Projectile.velocity.X *= 0.95f;
                                    float yLength = Math.Abs(nPC.Center.Y - Projectile.Center.Y);
                                    if (yLength > 500)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else if (yLength < 300)
                                        Helper.Movement_SimpleOneLine(ref Projectile.velocity.Y, -directionY, 6f, 0.18f, 0.25f, 0.97f);
                                    else
                                        Projectile.velocity.Y *= 0.96f;

                                    eyePosOffset = Vector2.Lerp(eyePosOffset, Projectile.velocity, 0.05f);
                                    eyeRot -= 0.1f;

                                    if (Timer > 20)
                                    {
                                        SonState++;
                                    }
                                    Timer++;
                                }
                                break;
                            case 3:
                                ResearchEnemy();
                                break;
                        }
                    }
                    break;
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<EsotericGuardianBuff>());
                return false;
            }

            if (owner.TryGetModPlayer(out EsotericGuardianPlayer egp) && egp.esotericGuardian)
                Projectile.timeLeft = 2;

            return true;
        }

        public void ResearchEnemy()
        {
            int target = AI_156_TryAttackingNPCs(Projectile);
            if (target != -1)
            {
                Target = target;
                Projectile.netUpdate = true;
                ResetState();
            }
            else
            {
                Timer = 0;
                State = (int)AIStates.back;
            }
        }

        public void ResetState()
        {
            Timer = 0;
            SonState = 0;
            State = GuardianType switch
            {
                (int)GuardianTypes.shoot => (int)AIStates.shoot,
                (int)GuardianTypes.magic => (int)AIStates.magic,
                _ => (float)(int)AIStates.slash,
            };
        }

        public int AI_156_TryAttackingNPCs(Projectile Projectile, bool skipBodyCheck = false)
        {
            Vector2 ownerCenter = Main.player[Projectile.owner].Center;
            int result = -1;
            float num = -1f;
            //如果有锁定的NPC那么就用锁定的，没有或不符合条件在从所有NPC里寻找
            NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
            if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this))
            {
                bool flag = true;
                if (!ownerMinionAttackTargetNPC.boss)
                    flag = false;

                if (ownerMinionAttackTargetNPC.Distance(ownerCenter) > 1000f)
                    flag = false;

                if (!skipBodyCheck && !Projectile.CanHitWithOwnBody(ownerMinionAttackTargetNPC))
                    flag = false;

                if (flag)
                    return ownerMinionAttackTargetNPC.whoAmI;
            }

            for (int i = 0; i < 200; i++)
            {
                NPC nPC = Main.npc[i];
                if (nPC.CanBeChasedBy(Projectile))
                {
                    float npcDistance2Owner = nPC.Distance(ownerCenter);
                    if (npcDistance2Owner <= 1000f && (npcDistance2Owner <= num || num == -1f) && (skipBodyCheck || Projectile.CanHitWithOwnBody(nPC)))
                    {
                        num = npcDistance2Owner;
                        result = i;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取自身是第几个召唤物弹幕
        /// 非常好的东西，建议稍微改改变成静态帮助方法
        /// </summary>
        /// <param name="Projectile"></param>
        /// <param name="index"></param>
        /// <param name="totalIndexesInGroup"></param>
        public void AI_GetMyGroupIndexAndFillBlackList(Projectile Projectile, out int index, out int totalIndexesInGroup)
        {
            index = 0;
            totalIndexesInGroup = 0;
            for (int i = 0; i < 1000; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Projectile.owner && projectile.type == Projectile.type && (projectile.type != 759 || projectile.frame == Main.projFrames[projectile.type] - 1))
                {
                    if (Projectile.whoAmI > i)
                        index++;

                    totalIndexesInGroup++;
                }
            }
        }

        public Vector2 CircleMovement(float speedMax, float maxDistance, float accelFactor = 0.25f, float angleFactor = 0.08f)
        {
            int timer = (int)(Main.GlobalTimeWrappedHourly / 4 % 3);
            Vector2 center = Owner.Center + (-MathHelper.PiOver2 + timer * MathHelper.TwoPi / 3f + GuardianType * MathHelper.TwoPi / 3f).ToRotationVector2() * (54 + Owner.velocity.Length() * 1.5f);
            //添加额外旋转
            center += (Main.GlobalTimeWrappedHourly / 3f * MathHelper.TwoPi + GuardianType * MathHelper.TwoPi / 3f).ToRotationVector2() * 12;

            //Vector2 center = Owner.Center + (baseRot + Main.GlobalTimeWrappedHourly / rollingFactor * MathHelper.TwoPi).ToRotationVector2() * distance;
            Vector2 dir = center - Projectile.Center;

            float velRot = Projectile.velocity.ToRotation();
            float targetRot = dir.ToRotation();

            float speed = Projectile.velocity.Length();
            float aimSpeed = Math.Clamp(dir.Length() / maxDistance, 0, 1) * speedMax;

            Projectile.velocity = velRot.AngleTowards(targetRot, angleFactor).ToRotationVector2() * Helper.Lerp(speed, aimSpeed, accelFactor);
            return center;
        }

        #region 绘制

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D mainTex = TextureAssets.Projectile[Type].Value;

            SpriteEffects effect = SpriteEffects.None;
            if (Projectile.spriteDirection < 0)
            {
                effect = SpriteEffects.FlipHorizontally;
            }

            var frame = mainTex.Frame(3, 2, (int)GuardianType, 0);
            Vector2 origin = frame.Size() / 2;

            if (ShadowOffset > 0)
            {
                for (int i = -1; i < 2; i += 2)
                    Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition + new Vector2(0, 6 * i * MathF.Sin(ShadowOffset * MathHelper.Pi)), frame,
                                            new Color(249, 101, 189, 0)*0.3f, Projectile.rotation, origin, Projectile.scale*1.1f, effect, 0f);
            }

            Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, frame,
                                    lightColor, Projectile.rotation, origin, Projectile.scale, effect, 0f);

            //绘制核心
            Texture2D coreTex = Request<Texture2D>(AssetDirectoryEX.MysteryGelItems + "EsotericGuardianCore").Value;
            Main.spriteBatch.Draw(coreTex, Projectile.Center + eyePosOffset - Main.screenPosition, null,
                                    lightColor, eyeRot, coreTex.Size() / 2, Projectile.scale, effect, 0f);

            frame = mainTex.Frame(3, 2, (int)GuardianType, 1);

            Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, frame,
                                    lightColor, Projectile.rotation, origin, Projectile.scale, effect, 0f);

            return false;
        }

        #endregion
    }

    /// <summary>
    /// 使用ai0输入弹幕持有者，ai1输入初始角度
    /// </summary>
    public class EsotericGuardianSlash : BaseSwingProj, IDrawWarp, IDrawAdditive
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public ref float OwnerIndex => ref Projectile.ai[0];
        public ref float StartRot => ref Projectile.ai[1];
        public ref float Combo => ref Projectile.ai[2];

        public EsotericGuardianSlash() : base(0f, 16) { }

        public static Asset<Texture2D> WarpTexture;
        public static Asset<Texture2D> GradientTexture;

        public int delay;
        public int alpha;

        public Vector2 scale;
        private float scaleOffset = 0.3f;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            WarpTexture = Request<Texture2D>(AssetDirectory.OtherProjectiles + "WarpTex");
            GradientTexture = Request<Texture2D>(AssetDirectoryEX.MysteryGelItems + "EsotericKeyGradient");
        }

        public override void SetSwingProperty()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.localNPCHitCooldown = 30;
            Projectile.width = 40;
            Projectile.height = 70;
            trailTopWidth = 20;
            distanceToOwner = 40;
            trailBottomWidth = 80;
            minTime = 0;
            useSlashTrail = true;
        }

        protected override void InitBasicValues()
        {
            if (!Main.projectile.IndexInRange((int)OwnerIndex))
            {
                Projectile.Kill();
                return;
            }

            Projectile proj = Main.projectile[(int)OwnerIndex];
            if (!proj.active)
                Projectile.Kill();

            scale = new Vector2(0f, 0);
            Projectile.extraUpdates = 2;
            startAngle = 0f;
            minTime = 40;
            maxTime = 25 * 3;
            int projCount = Owner.ownedProjectileCounts[ProjectileType<EsotericGuardianController>()];
            Projectile.height +=projCount * 8;
            distanceToOwner += projCount * 4;
            scaleOffset += 0.03f * projCount;

            switch (Combo)
            {
                default:
                case 0:
                    totalAngle = 4.4f * proj.direction;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;

                    break;
                case 1:
                    totalAngle = -4.4f * proj.direction;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;

                    break;
                case 2:
                    totalAngle = 3.7f * proj.direction;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;
                    break;
                case 3:
                    totalAngle = -3.7f * proj.direction;
                    Smoother = Coralite.Coralite.Instance.BezierEaseSmoother;

                    break;
            }
            delay = 24;
            alpha = 0;

            Projectile.velocity *= 0f;
            if (Owner.whoAmI == Main.myPlayer)
            {
                _Rotation = startAngle = GetStartAngle() - proj.direction * startAngle;//设定起始角度
                totalAngle *= proj.direction;
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

        protected override float GetStartAngle()
        {
            return StartRot;
        }

        protected override float ControlTrailBottomWidth(float factor)
        {
            return Projectile.height;
        }

        protected override void AIBefore()
        {
            if (Main.projectile.IndexInRange((int)OwnerIndex))
            {
                Projectile proj = Main.projectile[(int)OwnerIndex];
                if (!proj.active || proj.ai[1] != (int)EsotericGuardian.AIStates.slash)
                    Projectile.Kill();
            }
        }

        protected override Vector2 OwnerCenter()
        {
            if (Main.projectile.IndexInRange((int)OwnerIndex))
                return Main.projectile[(int)OwnerIndex].Center;
            return base.OwnerCenter();
        }

        protected override void BeforeSlash()
        {
            Slasher();
            if (Main.rand.NextBool(6))
            {
                Dust.NewDustPerfect(Projectile.Center + RotateVec2 * Main.rand.NextVector2Circular(-Projectile.height * 0.4f, Projectile.height * 0.4f)
                    , DustType<GlowBall>(), RotateVec2 * Main.rand.NextFloat(0.5f, 1.5f),
                    0, Color.Pink, Main.rand.NextFloat(0.2f, 0.35f));
            }

            if (Timer < minTime / 2)
                scale = Vector2.Lerp(Vector2.Zero, new Vector2(1.5f, 2), Timer / (minTime / 2));
            else if (Timer < minTime)
                scale = Vector2.Lerp(new Vector2(1.5f, 2), new Vector2(1f, 1.1f), (Timer - minTime / 2) / (minTime / 2));
            else
            {
                Helper.PlayPitched("Misc/LaserSwing", 1f, 0f, Projectile.Center);
                SoundStyle st = CoraliteSoundID.LaserSwing_Item15;
                st.Volume = 1;
                SoundEngine.PlaySound(st, Projectile.Center);
            }
        }

        protected override void OnSlash()
        {
            int timer = (int)Timer - minTime;

            if (Main.rand.NextBool(2))//生成粒子
            {
                Dust d= Dust.NewDustPerfect(Projectile.Center + RotateVec2 * Main.rand.NextVector2Circular(-Projectile.height * 0.4f, Projectile.height * 0.4f)
                    , DustID.PinkSlime, RotateVec2.RotatedBy(1.57f) * Main.rand.NextFloat(1.5f, 3.5f),
                    150, Color.Pink, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(Projectile.Center + RotateVec2 * Main.rand.NextVector2Circular(-Projectile.height * 0.4f, Projectile.height * 0.4f)
                    , DustType<GlowBall>(), RotateVec2.RotatedBy(Math.Sign(totalAngle)*1.57f) * Main.rand.NextFloat(1f, 2.5f),
                    0, Color.Pink, Main.rand.NextFloat(0.3f, 0.55f));
            }

            if (timer % 20 == 0)
            {
                Helper.PlayPitched("Misc/LaserSwing", 1f, 0f, Projectile.Center);
                SoundStyle st = CoraliteSoundID.LaserSwing_Item15;
                st.Volume = 1;
                SoundEngine.PlaySound(st, Projectile.Center);
            }

            if (timer < minTime + 8)
            {
                alpha = (int)(Coralite.Coralite.Instance.X2Smoother.Smoother(timer - minTime, 8) * 255);
            }
            else
                alpha = 255;
            base.OnSlash();
        }

        protected override void AfterSlash()
        {
            if (alpha > 20)
                alpha -= 5;
            Slasher();
            if (Timer < maxTime + delay / 2)
                scale = Vector2.Lerp(scale, new Vector2(1.5f, 2f), 0.05f);
            else if (Timer < maxTime + delay)
                scale = Vector2.Lerp(scale, Vector2.Zero, 0.1f);
            else
                Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (onHitTimer == 0)
            {
                onHitTimer = 1;
            }

            if (Main.netMode == NetmodeID.Server)
                return;

            Dust dust;
            float offset = Projectile.localAI[1] + Main.rand.NextFloat(0, Projectile.width * Projectile.scale - Projectile.localAI[1]);
            Vector2 pos = Bottom + RotateVec2 * offset;
            if (VisualEffectSystem.HitEffect_Lightning)
            {
                byte hue = (byte)(0.8f * 255f);

                for (int i = 0; i < 6; i++)
                {
                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings
                    {
                        PositionInWorld = pos,
                        MovementVector = RotateVec2.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(6f, 18f),
                        UniqueInfoPiece = hue
                    });
                }
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
        }


        protected override Vector2 GetCenter(int i)
        {
            if (Main.projectile.IndexInRange((int)OwnerIndex))
                return Main.projectile[(int)OwnerIndex].Center;

            return Owner.Center;
        }

        public override void PostDraw(Color lightColor) { }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D mainTex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = mainTex.Size() / 2;
            Vector2 scale2 = scale * scaleOffset;
            spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, mainTex.Frame(),
                                    Color.White, Projectile.rotation, origin, scale2, 0, 0f);

            for (int i = -3; i < 3; i++)
            {
                spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition + RotateVec2 * i * 8, mainTex.Frame(),
                  new Color(254, 149, 210, 150),
                   Projectile.rotation, origin, scale2 * 1.5f, 0, 0f);
            }
        }

        public void DrawWarp()
        {
            WarpDrawer(1);
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
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, default, Main.GameViewMatrix.ZoomMatrix);

                Effect effect = Filters.Scene["SimpleGradientTrail"].GetShader().Shader;

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.TransformationMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(CoraliteAssets.Trail.SlashFlatBlur.Value);
                effect.Parameters["gradientTexture"].SetValue(GradientTexture.Value);

                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes) //应用shader，并绘制顶点
                {
                    pass.Apply();
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
                }

                Main.graphics.GraphicsDevice.RasterizerState = originalState;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }

    public class EsotericGuardianBullet : ModProjectile
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 400;
            Projectile.minionSlots = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]++;
            Projectile.netUpdate = true;

            //简易撞墙反弹
            float newVelX = Math.Abs(Projectile.velocity.X);
            float newVelY = Math.Abs(Projectile.velocity.Y);
            float oldVelX = Math.Abs(oldVelocity.X);
            float oldVelY = Math.Abs(oldVelocity.Y);
            if (oldVelX > newVelX)
                Projectile.velocity.X = -oldVelX * 0.8f;
            if (oldVelY > newVelY)
                Projectile.velocity.Y = -oldVelY * 0.8f;

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), DustID.PinkSlime,
                       -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(0.1f, 0.3f), 150, Scale: Main.rand.NextFloat(1f, 1.4f));
                dust.noGravity = true;
            }

            return Projectile.ai[0] > 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), DustID.PinkSlime,
                       -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.1f, 0.3f), 150, Scale: Main.rand.NextFloat(1f, 1.4f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.PinkSlime,
                      /* Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1),*/ Alpha: 150, Scale: Main.rand.NextFloat(1f, 1.4f));
            }
        }
    }

    /// <summary>
    /// 使用ai0输入持有者，ai1输入总共多少个，ai2输入自身是第几个
    /// </summary>
    public class EsotericGuardianMagicBall : ModProjectile
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public ref float OwnerIndex => ref Projectile.ai[0];
        public ref float Total => ref Projectile.ai[1];
        public ref float Self => ref Projectile.ai[2];

        public ref float Timer => ref Projectile.localAI[0];

        public Vector2 TruePos;
        public float distanceToPos;
        public float rotToPos;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 400;
            Projectile.minionSlots = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Timer++;

            if (!Main.projectile.IndexInRange((int)OwnerIndex))
            {
                Projectile.Kill();
                return;
            }

            Projectile projOwner = Main.projectile[(int)OwnerIndex];
            if (!projOwner.active)
            {
                Projectile.Kill();
                return;
            }

            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height), DustID.PinkSlime,
                       -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.1f, 0.3f), 150, Scale: Main.rand.NextFloat(1f, 1.4f));
                dust.noGravity = true;
            }

            do
            {
                if (Timer < 60)
                {
                    distanceToPos += 48 / 60f;
                    TruePos = projOwner.Center;

                    Projectile.rotation += 0.1f;
                    break;
                }

                if (Timer == 60)
                {
                    if (Main.npc.IndexInRange((int)projOwner.ai[2]))
                    {
                        NPC npc = Main.npc[(int)projOwner.ai[2]];
                        if (npc.active)
                            Projectile.velocity = (npc.Center - projOwner.Center).SafeNormalize(Vector2.Zero) * 9;
                    }
                }

                if (Timer == 80)
                {
                    Projectile.tileCollide = true;
                }

                TruePos += Projectile.velocity;
                Projectile.rotation += 0.15f;

            } while (false);

            rotToPos = Self / Total * MathHelper.TwoPi + Timer * 0.08f;
            Projectile.Center = TruePos + rotToPos.ToRotationVector2() * distanceToPos;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.PinkSlime,
                       Alpha: 150, Scale: Main.rand.NextFloat(1f, 1.4f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D mainTex = TextureAssets.Projectile[Type].Value;

            var frameBox = mainTex.Frame(1, 2, 0, 0);
            Projectile.DrawShadowTrailsSacleStep(lightColor, 0.5f, 0.5f / 8, 1, 8, 1, 0.05f, frameBox);

            var origin = frameBox.Size() / 2;

            Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, frameBox, lightColor, Projectile.rotation, origin, Projectile.scale, 0, 0);
            frameBox = mainTex.Frame(1, 2, 0, 1);
            Main.spriteBatch.Draw(mainTex, Projectile.Center - Main.screenPosition, frameBox, new Color(251, 192, 224) * 0.5f, Projectile.rotation, origin, Projectile.scale, 0, 0);
            return false;
        }
    }
}
