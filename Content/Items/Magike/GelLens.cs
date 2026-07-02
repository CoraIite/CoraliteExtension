using Coralite.Content.Items.Gels;
using Coralite.Content.Items.MagikeSeries2;
using Coralite.Content.UI.MagikeApparatusPanel;
using Coralite.Core;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.BaseItems;
using Coralite.Core.Systems.MagikeSystem.Components;
using Coralite.Core.Systems.MagikeSystem.Components.Producers;
using Coralite.Core.Systems.MagikeSystem.MagikeLevels;
using Coralite.Core.Systems.MagikeSystem.TileEntities;
using Coralite.Core.Systems.MagikeSystem.Tiles;
using Coralite.Helpers;
using CoraliteExtension.Content.Items.MysteryGel;
using CoraliteExtension.Content.Particles;
using CoraliteExtension.Core;
using InnoVault.PRT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Magike
{
    public class GelLens() : MagikeApparatusItem(TileType<GelLensTile>(), Item.sellPrice(silver: 20)
            , RarityType<MysteryGelRarity>(), AssetDirectoryEX.MagikeItems)
    {
        private static PRTGroup group;

        public static LocalizedText ProduceCondition {  get;private set; }
        public static LocalizedText LensName {  get;private set; }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            ProduceCondition=this.GetLocalization(nameof(ProduceCondition));
            LensName = this.GetLocalization(nameof(LensName));
        }

        public override void Unload()
        {
            ProduceCondition = null;
            LensName = null;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<CrystallineMagike>(8)
                .AddIngredient<MysteryGel.MysteryGel>(4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            group?.Update();
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                group ??= new PRTGroup();
                if (group != null)
                {
                    if (!Main.gamePaused && Main.GameUpdateCount % 10 == 0)
                    {
                        int type;
                        Color c;
                        float speed;
                        float scale;
                        if (Main.rand.NextBool(4))
                        {
                            type = CoraliteContent.ParticleType<GoldSparkle>();
                            c = Color.Pink;
                            speed = Main.rand.NextFloat(0.5f, 0.6f);
                            scale = Main.rand.NextFloat(0.5f, 0.9f);
                        }
                        else
                        {
                            type = CoraliteContent.ParticleType<MysteryGelParticle>();
                            c = Color.White;
                            speed = Main.rand.NextFloat(0.4f, 0.6f);
                            scale = Main.rand.NextFloat(0.8f, 1.2f);
                        }

                        Vector2 size = ChatManager.GetStringSize(line.Font, line.Text, line.BaseScale);
                        group.NewParticle(new Vector2(line.X, line.Y) + new Vector2(Main.rand.NextFloat(0, size.X), size.Y - 4),
                            Main.rand.NextFloat(-1.57f - 0.3f, -1.57f + 0.3f).ToRotationVector2() * speed, type, c, scale);
                    }
                }
                group?.DrawInUI(Main.spriteBatch);
            }

            return true;
        }
    }

    public class GelLensTile() : BaseLensTile
        (Color.Pink, DustID.PinkSlime)
    {
        public override string Texture => AssetDirectoryEX.MagikeTiles + Name;

        public override int DropItemType => ItemType<GelLens>();

        public override List<ushort> GetAllLevels()
        {
            return [
                NoneLevel.ID,
                BrilliantLevel.ID,
                ];
        }

        public override void DrawExtraTex(SpriteBatch spriteBatch, Texture2D tex, Rectangle tileRect, Vector2 offset, Color lightColor, float rotation, MagikeTP entity, ushort level)
        {
            Vector2 selfCenter = tileRect.Center();
            Vector2 drawPos = selfCenter + offset;
            int halfHeight = Math.Max(tileRect.Height / 2, tileRect.Width / 2);

            //虽然一般不会没有 但是还是检测一下
            if (!entity.TryGetComponent(MagikeComponentID.MagikeProducer, out MagikeProducer producer))
                return;

            bool canProduce = producer.CanProduce();

            if (canProduce)
            {
                const float TwoPi = (float)Math.PI * 2f;
                float offset2 = (float)Math.Sin((Main.GlobalTimeWrappedHourly + tileRect.X + tileRect.Y) * TwoPi / 5f);
                drawPos += new Vector2(0f, offset2 * 4f);
            }
            else
                drawPos -= rotation.ToRotationVector2() * (halfHeight - ((tileRect.Width > tileRect.Height ? tex.Width : tex.Height) / 2) - 4);

            if (!entity.TryGetComponent(MagikeComponentID.ItemContainer, out ItemContainer container))
                return;

            Color color = Color.White;

            foreach (var item in container.Items)
            {
                if (item.IsAir)
                    continue;

                if (item.type == ItemID.Gel)
                {
                    color = item.color;
                    color.A = 50;
                    break;
                }
                else if (item.type == ItemID.PinkGel)
                {
                    color = Color.HotPink;
                    color.A = 150;
                    break;
                }
                else if (item.type == ItemType<MysteryGel.MysteryGel>())
                {
                    color = RarityLoader.GetRarity(item.rare).RarityColor;
                    color.A = 150;
                    break;
                }
                else if (item.type == ItemType<EmperorGel>())
                {
                    color = Color.CornflowerBlue;
                    color.A = 150;
                    break;
                }
            }

            // 绘制主帖图
            DrawTopTex(spriteBatch, tex, drawPos, lightColor, level, canProduce);
            DrawTopTex(spriteBatch, tex, drawPos, lightColor.MultiplyRGBA(color), level, canProduce);
        }
    }

    public class GelLensTileEntity : BaseCostItemProducerTileEntity<GelLensTile>
    {
        public override MagikeContainer GetStartContainer()
            => new GelLensContainer();

        public override MagikeLinerSender GetStartSender()
            => new GelLensSender();

        public override MagikeCostItemProducer GetStartProducer()
            => new GelProducer();

        public override ItemContainer GetStartItemContainer()
            => new()
            {
                CapacityBase = 2
            };
    }

    public class GelLensContainer : UpgradeableContainer<GelLensTile>
    {
    }

    public class GelLensSender : UpgradeableLinerSender<GelLensTile>
    {
    }

    public class GelProducer : UpgradeableCostItemProducer<GelLensTile>
    {
        public override string GetCanProduceText => GelLens.ProduceCondition.Value;

        public override MagikeSystem.UITextID NameText { get => MagikeSystem.UITextID.ForestLensName; }

        public override bool CanConsumeItem(Item item)
            => item.type is ItemID.Gel or ItemID.PinkGel || item.type == ItemType<MysteryGel.MysteryGel>()|| item.type == ItemType<EmperorGel>();

        public override int GetMagikeAmount(Item item)
        {
            switch (item.type)
            {
                default:
                    if (item.type == ItemType<MysteryGel.MysteryGel>())
                        return 300;
                    else if (item.type == ItemType<EmperorGel>())
                        return 120;
                    break;
                case ItemID.Gel:
                    return 60;
                case ItemID.PinkGel:
                    return 80;
            }

            return 0;
        }

        public override void ShowInUI(UIElement parent)
        {
            UIElement uIElement = new ComponentUIElementText<GelProducer>(c =>
                GelLens.LensName.Value, this, parent, new Vector2(1.3f));
            parent.Append(uIElement);

            UIGrid uIGrid = ItemSlotGrid();
            uIGrid.SetSize(200f, 500f);

            UIList uIList = new UIList
            {
                new TimerProgressBar(this),
                this.NewTextBar((MagikeCostItemProducer c) => MagikeSystem.GetUIText(MagikeSystem.UITextID.ProduceCondition), parent),
                this.NewTextBar((MagikeCostItemProducer c) => "  ▶ " + c.GetCanProduceText, parent),
                uIGrid
            };

            uIList.SetSize(0f, 0f - uIElement.Height.Pixels, 1f, 1f);
            uIList.SetTopLeft(uIElement.Height.Pixels + 8f, 0f);
            uIList.QuickInvisibleScrollbar();
            parent.Append(uIList);
        }
    }
}
