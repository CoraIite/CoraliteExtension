using Coralite.Content.Items.Glistent;
using Coralite.Content.Items.Icicle;
using Coralite.Core;
using Coralite.Helpers;
using CoraliteExtension.Content.Compats;
using CoraliteExtension.Core;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CoraliteExtension.Content.Items.Clickers
{
    public class GlistentClicker() : BaseClickerWeapon(2.6f, Coralite.Coralite.GlistentGreen, DustID.GemEmerald, 7)
    {
        public override string Texture => AssetDirectoryEX.ClickerItems + Name;

        public static readonly int DamageRatioPercent = 75;

        public static string GlistentEffect { get; private set; } = string.Empty;

        //Optional, if you only want this item to exist only when Clicker Class is enabled
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ClickerCompat.ClickerClass != null;
        }

        public override void SetOtherStaticDefaults()
        {
            //Here we register a click effect which we reference in SetDefaults through AddEffect
            string uniqueName = ClickerCompat.RegisterClickEffect(Mod, nameof(GlistentEffect), 14, Coralite.Coralite.GlistentGreen,
                delegate (Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Dust d = Dust.NewDustPerfect(position+Main.rand.NextVector2Circular(24,24)
                            , DustID.GemEmerald, Helper.NextVec2Dir(1f, 2.5f), 150, Scale: Main.rand.NextFloat(1, 1.5f));
                        d.noGravity = true;
                    }

                    Helper.PlayPitched(CoraliteSoundID.WaterBolt_Item21, position,pitch:-0.5f);
                    Projectile.NewProjectile(source, position, Vector2.Zero,
                        ModContent.ProjectileType<GlistentClickerProj>(), (int)(damage * DamageRatioPercent / 100f), 0, Main.myPlayer,1);
                },
            preHardMode: true,
            descriptionArgs: new object[] { DamageRatioPercent });
            //The preHardMode parameter flags it as available in pre-hardmode, useful for content referencing other effects
            //The next two parameters are optional and used for localization usage with substitutions for display name and description. Here the amount of mini clickers spawned and the damage they deal is reflected in the description (found in the localization file) so we need to supply those

            //We want to cache the result to make accessing it easier in other places.
            //(Make sure to unload the saved string again!)
            GlistentEffect = uniqueName;
        }

        public override void SetOtherDefaults()
        {
            ClickerCompat.AddEffect(Item, $"{nameof(CoraliteExtension)}:{nameof(GlistentEffect)}");

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

    public class GlistentClickerDebuff : ModBuff
    {
        public override string Texture => AssetDirectory.Buffs + "Debuff";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(npc.getRect())
                    , DustID.GemEmerald, Helper.NextVec2Dir(0.5f, 1f));
                d.noGravity = true;
            }
        }
    }

    public class GlistentClickerGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (ClickerCompat.ClickerClass != null && npc.HasBuff<GlistentClickerDebuff>() && projectile.DamageType.CountsAsClass(ContentSamples.ItemsByType[ModContent.ItemType<GlistentClicker>()].DamageType) && projectile.type != ModContent.ProjectileType<GlistentClickerProj>())
            {
                int damage = hit.Damage / 2;
                if (damage > 20)
                    damage = 20;

                npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<GlistentClickerDebuff>()));
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), npc.Center, Vector2.Zero,
                    ModContent.ProjectileType<GlistentClickerProj>(), damage, 0, Main.myPlayer);
            }
        }

        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
        }
    }

    public class GlistentClickerProj : GlistentJarExplode
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            ClickerCompat.SetClickerProjectileDefaults(Projectile);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.95f);
            if (HasDebuff == 1)
            {
                target.AddBuff(ModContent.BuffType<GlistentClickerDebuff>(), 60 * 10);
                HasDebuff = 0;
                Projectile.netUpdate = true;
            }
        }
    }
}
