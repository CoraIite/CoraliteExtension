using Coralite.Core;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Misc
{
    public class TheDiceOfTheCreator : ModItem
    {
        public override string Texture => AssetDirectoryEX.MiscItems + Name;

        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 30;
            Item.UseSound = CoraliteSoundID.Eat_Item2;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Red;
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemTime<2)
            {
                int type = Main.rand.Next(1, ItemLoader.ItemCount);

               int index= Item.NewItem(Item.GetSource_FromThis(), player.Center + new Vector2(0, -64), type);
                Item item = Main.item[index];
                CombatText.NewText(new Rectangle((int)player.Top.X, (int)player.Top.Y, 1, 1), Color.White, "命运选择了 " + item.Name);

                Item.TurnToAir();
            }
            return true ;
        }
    }
}
