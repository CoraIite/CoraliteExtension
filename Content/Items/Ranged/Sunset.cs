using Coralite.Content.Items.RedJades;
using Coralite.Content.Items.ThyphionSeries;
using Coralite.Content.ModPlayers;
using Coralite.Content.RecipeGroups;
using Coralite.Core;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Ranged
{
    public class Sunset : ModItem, IDashable
    {
        public override string Texture => AssetDirectoryEX.RangedItems + Name;

        public float Priority => IDashable.HeldItemDash;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(18, 2f);
            Item.DefaultToRangedWeapon(10, AmmoID.Arrow, 24, 7f);

            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.value = Item.sellPrice(0, 0, 10);

            Item.noUseGraphic = true;

            Item.UseSound = CoraliteSoundID.Bow_Item5;
        }

        public override void HoldItem(Player player)
        {
            if (player.TryGetModPlayer(out CoralitePlayer cp))
            {
                cp.AddDash(this);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.One);
            float rot = dir.ToRotation();
            Projectile.NewProjectile(new EntitySource_ItemUse(player, Item), player.Center, Vector2.Zero, ProjectileType<SunsetHeldProj>(), damage, knockback, player.whoAmI, rot, 0);
            Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(WoodenBowGroup.GroupName)
                .AddIngredient<RedJade>(2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        public bool Dash(Player Player, int DashDir)
        {
            Vector2 newVelocity = Player.velocity;
            float dashDirection;
            switch (DashDir)
            {
                case CoralitePlayer.DashLeft:
                case CoralitePlayer.DashRight:
                    {
                        dashDirection = DashDir == CoralitePlayer.DashRight ? 1 : -1;
                        newVelocity.X = dashDirection * 10;
                        break;
                    }
                default:
                    return false;
            }

            Player.GetModPlayer<CoralitePlayer>().DashDelay = 80;
            Player.GetModPlayer<CoralitePlayer>().DashTimer = 16;
            Player.velocity = newVelocity;
            Player.direction = (int)dashDirection;

            if (Player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(CoraliteSoundID.Swing_Item1, Player.Center);

                foreach (var proj in from proj in Main.projectile
                                     where proj.active && proj.friendly && proj.owner == Player.whoAmI && proj.type == ProjectileType<SunsetHeldProj>()
                                     select proj)
                {
                    proj.Kill();
                    break;
                }

                //生成手持弹幕
                Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero, ProjectileType<SunsetHeldProj>(),
                    Player.HeldItem.damage, Player.HeldItem.knockBack, Player.whoAmI, 1.57f + dashDirection * 1, 1, 16);
            }

            return true;
        }
    }

   public class SunsetHeldProj : AfterglowHeldProj
    {
        public override string Texture => AssetDirectoryEX.RangedItems + "Sunset";

        public override int GetItemType()
             => ItemType<Sunset>();
    }
}
