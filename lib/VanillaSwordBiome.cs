using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Coralite.lib
{
    public class VanillaSwordBiome
    {
        public bool Place(Point origin, StructureMap structures)
        {
            //使用TileScanner，检查以原点为中心的50x50区域是否大部分是泥土或石头。
            Dictionary<ushort, int> tileDictionary = new Dictionary<ushort, int>();
            WorldUtils.Gen(
                new Point(origin.X - 25, origin.Y - 25),
                new Shapes.Rectangle(50, 50),
                new Actions.TileScanner(TileID.Dirt, TileID.Stone).Output(tileDictionary));

            //如果数量少于1250，则返回false，会重新选取一个原点。
            //(这个方法外如何取的原点可以查看源码)
            if (tileDictionary[TileID.Dirt] + tileDictionary[TileID.Stone] < 1250)
                return false; 

            Point surfacePoint;
            //向上搜索1000格，找到50格高、1格宽的没有实心物块的区域。基本上等于找到了地表。
            //这个WorldUtils.Find就是教程中没讲的东西（教程里直接硬写的检测）
            bool flag = WorldUtils.Find(
                origin,
                Searches.Chain(new Searches.Up(1000),
                new Conditions.IsSolid().AreaOr(1, 50).Not()), out surfacePoint);

            //从原点到地表进行搜索，确保之间没有沙子。
            if (WorldUtils.Find(
                origin,
                Searches.Chain(new Searches.Up(origin.Y - surfacePoint.Y),
                new Conditions.IsTile(TileID.Sand)), out Point _))
                return false;

            if (!flag)
                return false;

            surfacePoint.Y += 50;
            ShapeData slimeShapeData = new ShapeData();
            ShapeData moundShapeData = new ShapeData();
            Point point = new Point(origin.X, origin.Y + 20);
            Point point2 = new Point(origin.X, origin.Y + 30);
            float xScale = 0.8f + WorldGen.genRand.NextFloat() * 0.5f;
            //随机剑冢地形的宽度
            //检查剑冢的肺结构和结构图是否有冲突
            //这个结构图是GenVars.structures，其中记录了大部分受保护地形如蜂巢等，可以判断它来防止冲突
            if (!structures.CanPlace(new Rectangle(point.X - (int)(20f * xScale), point.Y - 20, (int)(40f * xScale), 40)))
                return false;
            //通往地面的竖井检查结构图中是否有任何冲突
            if (!structures.CanPlace(new Rectangle(origin.X, surfacePoint.Y + 10, 1, origin.Y - surfacePoint.Y - 9), 2))
                return false;


            //使用史莱姆形状清理物块。Blotches作用是让边缘随机起伏。 https://i.imgur.com/WtZaBbn.png
            //上面的网站是tml教程里就有的
            WorldUtils.Gen(
                point,
                new Shapes.Slime(20, xScale, 1f),
                Actions.Chain(
                    new Modifiers.Blotches(2, 0.4),
                    new Actions.ClearTile(frameNeighbors: true).Output(slimeShapeData)));


            //在切出的史莱姆形状内放置一个土堆。
            WorldUtils.Gen(
                point2,
                new Shapes.Mound(14, 14),
                Actions.Chain(
                    new Modifiers.Blotches(2, 1, 0.8),
                    new Actions.SetTile(TileID.Dirt),
                    new Actions.SetFrames(frameNeighbors: true).Output(moundShapeData)));


            //史莱姆形状减去小土堆形状，得到一个类似肺的形状
            slimeShapeData.Subtract(moundShapeData, point, point2);


            //沿着史莱姆形状的内边缘放置草方块
            WorldUtils.Gen(
                point,
                new ModShapes.InnerOutline(slimeShapeData),
                Actions.Chain(
                    new Actions.SetTile(TileID.Grass),
                    new Actions.SetFrames(frameNeighbors: true)));


            //在史莱姆形状的下半部分的空位置上放水
            WorldUtils.Gen(
                point,
                new ModShapes.All(slimeShapeData),
                Actions.Chain(
                    new Modifiers.RectangleMask(-40, 40, 0, 40),
                    new Modifiers.IsEmpty(),
                    new Actions.SetLiquid()));


            //在所有史莱姆形状内放花墙。在史莱姆形状的所有草方块下放置藤蔓。
            WorldUtils.Gen(
                point,
                new ModShapes.All(slimeShapeData),
                Actions.Chain(
                    new Actions.PlaceWall(WallID.Flower),
                    new Modifiers.OnlyTiles(TileID.Grass),
                    new Modifiers.Offset(0, 1),
                    new ActionVines(3, 5)));


            //向上打竖井，并且将沙子变为硬化沙子防止它掉下来
            ShapeData shaftShapeData = new ShapeData();
            WorldUtils.Gen(
                new Point(origin.X, surfacePoint.Y + 10),
                new Shapes.Rectangle(1, origin.Y - surfacePoint.Y - 9),
                Actions.Chain(
                    new Modifiers.Blotches(2, 0.2),
                    new Actions.ClearTile().Output(shaftShapeData),
                    new Modifiers.Expand(1),
                    new Modifiers.OnlyTiles(TileID.Sand),
                    new Actions.SetTile(TileID.HardenedSand).Output(shaftShapeData)));


            //设置物块帧
            WorldUtils.Gen(
                new Point(origin.X, surfacePoint.Y + 10),
                new ModShapes.All(shaftShapeData),
                new Actions.SetFrames(frameNeighbors: true));


            //有三分之一放置一个真附魔剑冢
            if (WorldGen.genRand.NextBool(3))
                WorldGen.PlaceTile(point2.X, point2.Y - 15, TileID.LargePiles2, mute: true, forced: false, -1, 17);
            else
                WorldGen.PlaceTile(point2.X, point2.Y - 15, TileID.LargePiles, mute: true, forced: false, -1, 15);
            //在草方块上种植物。
            WorldUtils.Gen(
                point2,
                new ModShapes.All(moundShapeData),
                Actions.Chain(
                    new Modifiers.Offset(0, -1),
                    new Modifiers.OnlyTiles(TileID.Grass),
                    new Modifiers.Offset(0, -1), new ActionGrass()));
            //将剑冢添加到结构图中，防止后生成的其他东西和剑冢冲突。
            structures.AddStructure(new Rectangle(point.X - (int)(20f * xScale), point.Y - 20, (int)(40f * xScale), 40), 4);
            return true;
        }
    }
}
