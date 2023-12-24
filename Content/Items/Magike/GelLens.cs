using Coralite.Content.Items.Magike.BasicLens;
using Coralite.Core;
using Coralite.Core.Prefabs.Items;
using Coralite.Core.Systems.MagikeSystem;
using Coralite.Core.Systems.MagikeSystem.Base;
using Coralite.Core.Systems.MagikeSystem.TileEntities;
using Coralite.Core.Systems.ParticleSystem;
using Coralite.Helpers;
using CoraliteExtension.Content.Items.MysteryGel;
using CoraliteExtension.Content.Particles;
using CoraliteExtension.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace CoraliteExtension.Content.Items.Magike
{
    public class GelLens : BaseMagikePlaceableItem, IMagikeGeneratorItem, IMagikeSenderItem
    {
        private static ParticleGroup group;

        public GelLens() : base(TileType<GelLensTile>(), Item.sellPrice(0, 0, 10, 0)
            , RarityType<MysteryGelRarity>(), 300, AssetDirectoryEX.MagikeItems)
        { }

        public override int MagikeMax => 800;
        public string SendDelay => "5";
        public int HowManyPerSend => 40;
        public int ConnectLengthMax => 5;
        public int HowManyToGenerate => -1;
        public string GenerateDelay => "9";

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<BrilliantLens>()
                .AddIngredient<MysteryGel.MysteryGel>(4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            group?.UpdateParticles();
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                group ??= new ParticleGroup();
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
                group?.DrawParticlesInUI(Main.spriteBatch);
            }

            return true;
        }
    }

    public class GelLensTile : BaseCostItemLensTile
    {
        public override string Texture => AssetDirectoryEX.MagikeTiles + Name;
        public override string TopTextureName => AssetDirectoryEX.MagikeTiles + Name + "_Top";

        public override void SetStaticDefaults()
        {
            Main.tileShine[Type] = 400;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true; //不会出现挖掘失败的情况
            TileID.Sets.IgnoredInHouseScore[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new int[3] {
                16,
                16,
                16
            };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<GelLensEntity>().Hook_AfterPlacement, -1, 0, true);

            TileObjectData.addTile(Type);

            AddMapEntry(Color.Pink);
            DustType = DustID.PinkSlime;
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //这是特定于照明模式的，如果您手动绘制瓷砖，请始终包含此内容
            Vector2 offScreen = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen)
                offScreen = Vector2.Zero;

            //检查物块它是否真的存在
            Point p = new Point(i, j);
            Tile tile = Main.tile[p.X, p.Y];
            if (tile == null || !tile.HasTile)
                return;

            //获取初始绘制参数
            Texture2D texture = TopTexture.Value;

            // 根据项目的地点样式拾取图纸上的框架
            int frameY = tile.TileFrameX / FrameWidth;
            Rectangle frame = texture.Frame(HorizontalFrames, VerticalFrames, 0, frameY);

            Vector2 origin = frame.Size() / 2f;
            Vector2 worldPos = p.ToWorldCoordinates(halfWidth, halfHeight);

            Color color = Lighting.GetColor(p.X, p.Y);
           float lightLevel= Lighting.Brightness(p.X, p.Y);
            //这与我们之前注册的备用磁贴数据有关
            bool direction = tile.TileFrameY / FrameHeight != 0;
            SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 一些数学魔法，使其随着时间的推移平稳地上下移动
            Vector2 drawPos = worldPos + offScreen - Main.screenPosition;
            if (MagikeHelper.TryGetEntityWithTopLeft(i, j, out GelLensEntity container))
            {
                if (container.Active)   //如果处于活动状态那么就会上下移动，否则就落在底座上
                {
                    const float TwoPi = (float)Math.PI * 2f;
                    float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
                    drawPos += new Vector2(0f, offset * 4f);
                }
                else
                    drawPos += new Vector2(0, halfHeight - 16);

                if (container.itemToCosume != null && !container.itemToCosume.IsAir)
                {
                    Item item = container.itemToCosume;
                    spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

                    if (item.type == ItemID.Gel)
                        color = item.color;
                    else if (item.type == ItemID.PinkGel)
                        color = Color.Pink;
                    else if (item.type == ItemType<MysteryGel.MysteryGel>())
                        color = RarityLoader.GetRarity(item.rare).RarityColor;

                    color.A = 50;
                    color *= lightLevel;
                    spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);
                }
            }

            // 绘制主帖图
            spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);
        }
    }

    public class GelLensEntity : MagikeGenerator_FromMagItem
    {
        public const int sendDelay = 60 * 5;
        public int sendTimer;
        public GelLensEntity() : base(800, 5 * 16, 60 * 9) { }

        public override ushort TileType => (ushort)TileType<CrystalLensTile>();

        public override int HowManyPerSend => 40;

        public override int HowManyToGenerate
        {
            get
            {
                switch (itemToCosume.type)
                {
                    default:
                        if (itemToCosume.type == ItemType<MysteryGel.MysteryGel>())
                            return 115;
                        break;
                    case ItemID.Gel: return 20;
                    case ItemID.PinkGel: return 30;
                }

                return 0;
            }
        }

        public override bool CanSend()
        {
            sendTimer++;
            if (sendTimer > sendDelay)
            {
                sendTimer = 0;
                return true;
            }

            return false;
        }

        public override bool CanInsertItem(Item item)
        {
            return item.ammo == AmmoID.Gel || item.type == ItemID.PinkGel;
        }

        public override void SendVisualEffect(IMagikeContainer container)
        {
            MagikeHelper.SpawnDustOnSend(2, 3, Position, container, Color.Pink);
        }

        public override void OnReceiveVisualEffect()
        {
            MagikeHelper.SpawnDustOnGenerate(2, 3, Position, Color.Pink);
        }
    }
}
