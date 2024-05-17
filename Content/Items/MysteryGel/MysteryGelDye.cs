using Coralite.Core;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.CraftConditions;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.MysteryGel
{
    public class MysteryGelDye : BaseMysteryGelItem, IMagikePolymerizable
    {
        public override string Texture => AssetDirectoryEX.MysteryGelItems + Name;

        public sealed override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                GameShaders.Armor.BindShader(Item.type
                    , new ArmorShaderData(CoraliteExtension.Instance.Assets.Request<Effect>("Effects/GelFlower", ReLogic.Content.AssetRequestMode.ImmediateLoad), "ArmorMyShader"));
            }
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 25);
            Item.rare = ModContent.RarityType<MysteryGelRarity>();
        }

        public void AddMagikePolymerizeRecipe()
        {
            PolymerizeRecipe.CreateRecipe<MysteryGelDye>(200)
                .SetMainItem<MysteryGel>()
                .AddIngredient(ItemID.Gel, 49)
                .AddIngredient(ItemID.PinkGel, 9)
                .AddIngredient(ItemID.GelBalloon, 9)
                .AddCondition(HardModeCondition.Instance)
                .Register();
        }
    }
}
