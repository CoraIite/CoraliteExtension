using CoraliteExtension.Content.Compats;
using CoraliteExtension.Core;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public abstract class BaseClickerWeapon(float radius,Color color,int dustID,int damage) : ModItem
    {
        public override string Texture => AssetDirectoryEX.ClickerItems + Name;

        //Optional, if you only want this item to exist only when Clicker Class is enabled
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ClickerCompat.ClickerClass != null;
        }

        public override void SetStaticDefaults()
        {
            //You NEED to call this in SetStaticDefaults to make it count as a clicker weapon
            ClickerCompat.RegisterClickerWeapon(this, borderTexture: Texture + "_Outline");

            SetOtherStaticDefaults();
        }

        public virtual void SetOtherStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            //This call is mandatory as it sets common stats like useStyle which is shared between all clickers
            ClickerCompat.SetClickerWeaponDefaults(Item);

            //Use these methods to adjust clicker weapon specific stats
            ClickerCompat.SetRadius(Item, radius);
            ClickerCompat.SetColor(Item, color);
            ClickerCompat.SetDust(Item, dustID);

            Item.damage = damage;
            Item.width = 30;
            Item.height = 30;
            Item.knockBack = 0.5f;

            SetOtherDefaults();
        }

        public virtual void SetOtherDefaults()
        {

        }
    }
}
