using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;


namespace EvilExpansionMod.Content.Misc.Paintings;
public class VictorySignTile : ModTile {
    public override string Texture => Assets.Images.Paintings.VictorySignTile.KEY;
    public override void SetStaticDefaults() {

        Main.tileSolid[Type] = false;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = false;

        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorWall = true;

        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.Origin = new Point16(0, 1);

        TileObjectData.newTile.CoordinatePadding = 2;


        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 3;
        // Before that, you can make some changes to newTile like height, origin and etc.
        TileObjectData.addTile(Type);
    // AddMapEntry is for setting the color and optional text associated with the Tile when viewed on the map
    LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(238, 145, 105), name);
        }
}

    internal class VictorySignItem : ModItem {
    public override string Texture => Assets.Images.Paintings.VictorySign.KEY;
    public override void SetDefaults() {
        (Item.width, Item.height) = (32, 32);
        Item.sellPrice(0,2,0,0);
        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
        Item.createTile = ModContent.TileType<VictorySignTile>();
        }
    }