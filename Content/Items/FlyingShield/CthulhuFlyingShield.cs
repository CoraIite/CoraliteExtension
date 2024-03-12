using Coralite.Content.ModPlayers;
using Coralite.Core;
using Coralite.Core.Prefabs.Items;
using Coralite.Core.Prefabs.Projectiles;
using Coralite.Helpers;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.FlyingShield
{
    public class CthulhuFlyingShield : BaseFlyingShieldItem<CthulhuFlyingShieldGuard>, IBuffHeldItem
    {
        public CthulhuFlyingShield() : base(Item.sellPrice(0, 0, 0, 10), ItemRarityID.Expert, AssetDirectoryEX.FlyingShieldItems)
        {
        }

        public override void SetDefaults2()
        {
            Item.useTime = Item.useAnimation = 15;
            Item.shoot = ModContent.ProjectileType<CthulhuFlyingShieldProj>();
            Item.knockBack = 8;
            Item.shootSpeed = 14.5f;
            Item.damage = 27;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;
        public override bool ConsumeItem(Player player) => false;

        public override void RightClick(Player player)
        {
            Item.SetDefaults(ItemID.EoCShield);
            Item.stack = 2;
        }

        public void UpdateBuffHeldItem(Player player)
        {
            player.dashType = 2;
        }

        public override void HoldItem(Player player)
        {
            if (player.ItemTimeIsZero && player.ownedProjectileCounts[Item.shoot] == 0)
                player.GetModPlayer<CthulhuFlyingShieldPlayer>().handEocShield = true;
        }
    }

    public class CthulhuFlyingShieldPlayer:ModPlayer
    {
        public bool handEocShield;

        public override void ResetEffects()
        {
            handEocShield = false;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (handEocShield)
                drawInfo.drawPlayer.shield = 5;
        }
    }

    public class CthulhuFlyingShieldProj : BaseFlyingShield
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldItems + "CthulhuFlyingShieldProj";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 40;
        }

        public override void OnShootDusts()
        {
            extraRotation += 0.4f;
        }

        public override void OnBackDusts()
        {
            extraRotation += 0.4f;
        }

        public override void SetOtherValues()
        {
            flyingTime = 20;
            backTime = 6;
            backSpeed = 14.5f;
            trailCachesLength = 9;
            trailWidth = 16 / 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 direction = (Projectile.Center - target.Center).SafeNormalize(Vector2.One);

            Helper.SpawnDirDustJet(target.Center, () => direction.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), 2, 12,
                (i) => i * 1f, DustID.Blood, Scale: Main.rand.NextFloat(1f, 2f), noGravity: false, extraRandRot: 0.1f);

            for (int i = 0; i < 10; i++)
                Dust.NewDustPerfect(target.Center, DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(2f, 4f),
                    Scale: Main.rand.NextFloat(1f, 2f));

            base.OnHitNPC(target, hit, damageDone);
        }

        public override void DrawTrails(Color lightColor)
        {
            Texture2D mainTex = Projectile.GetTexture();
            var origin = mainTex.Size() / 2;
            for (int i = trailCachesLength - 1; i > 4; i--)
                Main.spriteBatch.Draw(mainTex, Projectile.oldPos[i] - Main.screenPosition, null,
                lightColor * 0.6f * ((i - 4) * 1 / 3f), Projectile.oldRot[i] - 1.57f + extraRotation, origin, Projectile.scale, 0, 0);

            base.DrawTrails(lightColor);
        }

        public override Color GetColor(float factor)
        {
            return Color.DarkRed * factor;
        }
    }

    public class CthulhuFlyingShieldGuard : BaseFlyingShieldGuard
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldItems + "CthulhuFlyingShieldGuard";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 42;
            Projectile.height = 42;
        }

        public override void SetOtherValues()
        {
            damageReduce = 0.2f;
        }

        public override void OnGuard()
        {
            DistanceToOwner /= 3;
            SoundEngine.PlaySound(CoraliteSoundID.Fleshy2_NPCHit7, Projectile.Center);
        }

        public override float GetWidth()
        {
            return Projectile.width / 2 / Projectile.scale + 2;
        }

        public override void DrawSelf(Texture2D mainTex, Vector2 pos, float rotation, Color lightColor, Vector2 scale, SpriteEffects effect)
        {
            Rectangle frameBox;
            Vector2 rotDir = Projectile.rotation.ToRotationVector2();
            Vector2 dir = rotDir * (DistanceToOwner / (Projectile.width * scalePercent));
            Color c = lightColor * 0.6f;
            c.A = lightColor.A;

            frameBox = mainTex.Frame(3, 1, 0, 0);
            Vector2 origin2 = frameBox.Size() / 2;

            //绘制基底
            Main.spriteBatch.Draw(mainTex, pos - dir * 5, frameBox, c, rotation, origin2, scale, effect, 0);
            Main.spriteBatch.Draw(mainTex, pos, frameBox, lightColor, rotation, origin2, scale, effect, 0);

            //绘制上部
            frameBox = mainTex.Frame(3, 1, 1, 0);
            Main.spriteBatch.Draw(mainTex, pos + dir * 2, frameBox, c, rotation, origin2, scale, effect, 0);
            Main.spriteBatch.Draw(mainTex, pos + dir * 7, frameBox, lightColor, rotation, origin2, scale, effect, 0);

            //绘制上上部
            frameBox = mainTex.Frame(3, 1, 2, 0);
            Main.spriteBatch.Draw(mainTex, pos + dir * 7, frameBox, c, rotation, origin2, scale, effect, 0);
            Main.spriteBatch.Draw(mainTex, pos + dir * 12, frameBox, lightColor, rotation, origin2, scale, effect, 0);
        }
    }
}
