using Coralite.Content.Items.FlyingShields.Accessories;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.FlyingShield
{
    public class FlyingShieldGlobalItem : GlobalItem,ILocalizedModType
    {
        public bool justTransformed;
        public override bool InstancePerEntity => true;

        public string LocalizationCategory => "EXSystem";

        public static LocalizedText FlyingShieldExchange;

        public override void Load()
        {
            FlyingShieldExchange = this.GetLocalization("TransformTips", () => "[c/4f80bd:物品栏内右键以转换为飞盾]");
        }

        public override bool CanRightClick(Item item)
        {
            return item.type switch
            {
                ItemID.EoCShield => true,
                ItemID.CobaltShield => true,
                _ => base.CanRightClick(item),
            };
        }

        public override void RightClick(Item item, Player player)
        {
            if (justTransformed)
            {
                justTransformed = false;
                return;
            }

            switch (item.type)
            {
                default:
                    break;
                case ItemID.EoCShield:
                    item.SetDefaults(ModContent.ItemType<CthulhuFlyingShield>());
                    item.stack = 2;
                    break;
                case ItemID.CobaltShield:
                    item.SetDefaults(ModContent.ItemType<CobaltFlyingShield>());
                    item.stack = 2;
                    break;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            switch (item.type)
            {
                default:
                    break;
                case ItemID.EoCShield:
                case ItemID.CobaltShield:
                    {
                        TooltipLine line = new TooltipLine(Mod, "TransformTips", FlyingShieldExchange.Value);
                        tooltips.Add(line);
                    }
                    break;
            }
        }
    }
}
