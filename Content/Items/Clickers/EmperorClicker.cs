using Coralite.Content.Items.Gels;
using Coralite.Core;
using CoraliteExtension.Content.Compats;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class EmperorClicker() : BaseClickerWeapon(3.2f, Color.SkyBlue, DustID.Water, 9)
    {
        public static readonly int GelBallAmount = 4;
        public static readonly int DamageRatioPercent = 50;

        public static string EmperorEffect { get; private set; } = string.Empty;

        public override void SetOtherStaticDefaults()
        {
            string uniqueName = ClickerCompat.RegisterClickEffect(Mod, nameof(EmperorEffect), 10, Color.SkyBlue,
                delegate (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
                {
                    SoundEngine.PlaySound(CoraliteSoundID.Fleshy_NPCHit1, position);
                    float rot = Main.rand.NextFloat(MathHelper.TwoPi);
                    damage = (int)(damage * DamageRatioPercent / 100f);

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 dir = rot.ToRotationVector2();
                        Projectile.NewProjectile(source, position + (dir * Main.rand.NextFloat(60, 80)), dir * Main.rand.NextFloat(2, 4), ModContent.ProjectileType<GelChaserClicker>(), damage, knockBack, player.whoAmI, ai1: rot + MathHelper.Pi);
                        rot += Main.rand.NextFloat(MathHelper.PiOver2 - 0.3f, MathHelper.PiOver2 + 0.3f);
                    }
                },
            preHardMode: true,
            descriptionArgs: new object[] { GelBallAmount, DamageRatioPercent });

            EmperorEffect = uniqueName;
        }

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, $"{nameof(CoraliteExtension)}:{nameof(EmperorEffect)}");

            Item.SetShopValues(Terraria.Enums.ItemRarityColor.Orange3, Item.sellPrice(0, 1));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EmperorGel>(8)
                .AddTile(TileID.Solidifier)
                .Register();
        }
    }

    public class GelChaserClicker : GelChaser
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            ClickerCompat.SetClickerProjectileDefaults(Projectile);
        }
    }
}
