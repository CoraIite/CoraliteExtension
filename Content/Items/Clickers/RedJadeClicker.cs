using Coralite.Content.Items.Icicle;
using Coralite.Content.Items.RedJades;
using Coralite.Helpers;
using CoraliteExtension.Content.Compats;
using CoraliteExtension.Core;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class RedJadeClicker() : BaseClickerWeapon(1.8f, Coralite.Coralite.RedJadeRed, DustID.GemRuby, 5)
    {
        public static readonly int ExplodeChance = 50;
        public static readonly int DamagePercent = 75;

        public static string RedJadeEffect { get; private set; } = string.Empty;

        public override void SetOtherStaticDefaults()
        {
            string uniqueName = ClickerCompat.RegisterClickEffect(Mod, nameof(RedJadeEffect), 4, Coralite.Coralite.RedJadeRed,
                delegate (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
                {
                    if (Main.rand.NextBool())
                    {
                        Helper.PlayPitched("RedJade/RedJadeBoom", 0.4f, 0f, position);
                        Projectile.NewProjectile(source, position, Vector2.Zero,
                            ModContent.ProjectileType<RedJadeBoom>(), (int)(damage * DamagePercent / 100f), 0, Main.myPlayer);
                    }

                },
            preHardMode: true,
            descriptionArgs: new object[] { ExplodeChance, DamagePercent });

            RedJadeEffect = uniqueName;
        }

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, $"{nameof(CoraliteExtension)}:{nameof(RedJadeEffect)}");

            Item.SetShopValues(Terraria.Enums.ItemRarityColor.Blue1, Item.sellPrice(0, 0, 50));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RedJade>()
                .AddIngredient<IcicleBreath>(2)
                .AddTile<Coralite.Content.Tiles.RedJades.MagicCraftStation>()
                .Register();
        }
    }
}
