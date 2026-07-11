using Coralite.Content.Items.MagikeSeries1;
using Coralite.Content.Raritys;
using CoraliteExtension.Content.Compats;
using CoraliteExtension.Core;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class MagicCrystalClicker() : BaseClickerWeapon(1.8f, Coralite.Coralite.MagicCrystalPink, DustID.PinkTorch, 7)
    {
        public override string Texture => AssetDirectoryEX.ClickerItems + Name;

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, "ClickerClass:DoubleClick");

            Item.SetShopValues(Terraria.Enums.ItemRarityColor.Green2, Item.sellPrice(0, 0, 20));

            Item.rare = ModContent.RarityType<MagicCrystalRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MagicCrystal>(5)
                .AddIngredient<Basalt>(10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
