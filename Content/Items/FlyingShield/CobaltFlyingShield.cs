using Coralite.Content.ModPlayers;
using Coralite.Core.Prefabs.Items;
using Coralite.Core.Systems.FlyingShieldSystem;
using CoraliteExtension.Content.ModPlayers;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.FlyingShield
{
    public class CobaltFlyingShield : BaseFlyingShieldItem<CobaltFlyingShieldGuard>, IBuffHeldItem
    {
        public CobaltFlyingShield() : base(Item.sellPrice(0, 0, 50), ItemRarityID.Orange, AssetDirectoryEX.FlyingShieldItems)
        {
        }

        public override void SetDefaults2()
        {
            Item.useTime = Item.useAnimation = 15;
            Item.shoot = ModContent.ProjectileType<CobaltFlyingShieldProj>();
            Item.knockBack = 2;
            Item.shootSpeed = 15;
            Item.damage = 35;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.TryGetModPlayer(out CoralitePlayer cp))
            {
                cp.MaxFlyingShield++;
            }
            return base.CanUseItem(player);
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            Item.SetDefaults(ItemID.CobaltShield);
            Item.stack = 2;
            if (Item.TryGetGlobalItem(out FlyingShieldGlobalItem fsgi))
            {
                fsgi.justTransformed = true;
            }
        }

        public void UpdateBuffHeldItem(Player player)
        {
            player.statDefense += 1;
            player.noKnockback = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.ItemTimeIsZero && player.ownedProjectileCounts[Item.shoot] == 0)
                player.GetModPlayer<CoraliteEXPlayer>().AddEffect(nameof(CobaltFlyingShield));
        }
    }

    public class CobaltFlyingShieldProj : BaseFlyingShield
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldItems + "CobaltFlyingShield";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = Projectile.height = 36;
        }

        public override void SetOtherValues()
        {
            flyingTime = 20;
            backTime = 6;
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

    public class CobaltFlyingShieldGuard : BaseFlyingShieldGuard
    {
        public override string Texture => AssetDirectoryEX.FlyingShieldItems + "CobaltFlyingShield";

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 38;
            Projectile.height = 38;
        }

        public override void SetOtherValues()
        {
            damageReduce = 0.3f;
            scalePercent = 2f;
        }

        public override float GetWidth()
        {
            return Projectile.width * 0.5f / Projectile.scale;
        }
    }
}
