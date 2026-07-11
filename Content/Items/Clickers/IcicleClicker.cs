using Coralite.Content.Items.Icicle;
using Coralite.Core;
using CoraliteExtension.Content.Compats;
using CoraliteExtension.Core;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class IcicleClicker() : BaseClickerWeapon(2.8f, Coralite.Coralite.IcicleCyan, DustID.ApprenticeStorm, 17)
    {
        public override string Texture => AssetDirectoryEX.ClickerItems + Name;

        public static readonly int IcicleAmount = 3;
        public static readonly int DamageRatioPercent = 125;

        public static string IcicleEffect { get; private set; } = string.Empty;

        public override void SetOtherStaticDefaults()
        {
            //Here we register a click effect which we reference in SetDefaults through AddEffect
            string uniqueName = ClickerCompat.RegisterClickEffect(Mod, nameof(IcicleEffect), 8, Coralite.Coralite.IcicleCyan,
                delegate (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
                {
                    SoundEngine.PlaySound(CoraliteSoundID.IceMagic_Item28, position);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 center = position - new Vector2(0, Main.rand.Next(140, 220)).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                        Vector2 velocity = (position + Main.rand.NextVector2Circular(24, 24) - center).SafeNormalize(Vector2.UnitY) * 12;
                        Projectile.NewProjectile(source, center, velocity,
                            ModContent.ProjectileType<IcicleFalling>(), (int)(damage * DamageRatioPercent / 100f), 0, Main.myPlayer);
                    }
                },
            preHardMode: true,
            descriptionArgs: new object[] { IcicleAmount, DamageRatioPercent });
            //The preHardMode parameter flags it as available in pre-hardmode, useful for content referencing other effects
            //The next two parameters are optional and used for localization usage with substitutions for display name and description. Here the amount of mini clickers spawned and the damage they deal is reflected in the description (found in the localization file) so we need to supply those

            //We want to cache the result to make accessing it easier in other places.
            //(Make sure to unload the saved string again!)
            IcicleEffect = uniqueName;
        }

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, $"{nameof(CoraliteExtension)}:{nameof(IcicleEffect)}");

            Item.SetShopValues(Terraria.Enums.ItemRarityColor.Green2, Item.sellPrice(0, 1));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<IcicleCrystal>()
                .AddIngredient<IcicleBreath>(2)
                .AddTile(TileID.IceMachine)
                .Register();
        }
    }
}
