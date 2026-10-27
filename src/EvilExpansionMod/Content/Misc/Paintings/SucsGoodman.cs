
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
public class SucsGoodmanTile : ModTile {
    public override string Texture => Assets.Images.Paintings.SucsGoodmanTile.KEY;
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
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16];
        TileObjectData.newTile.Origin = new Point16(0, 3);

        TileObjectData.newTile.CoordinatePadding = 2;


        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.Height = 7;
        // Before that, you can make some changes to newTile like height, origin and etc.
        TileObjectData.addTile(Type);
        AnimationFrameHeight = 126;
    // AddMapEntry is for setting the color and optional text associated with the Tile when viewed on the map
    LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(238, 145, 105), name);
    }

    bool isOnScreen;
    int timer = 0;
    int cooldownTimer = 0;
    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        if(Main.tile[i, j].TileType == ModContent.TileType<SucsGoodmanTile>() && Main.tile[i, j].HasTile) {
            if(Main.rand.NextBool(50000) && isOnScreen && cooldownTimer == 0) {
                timer = 80;
            }
            if(timer != 0) {
                frameYOffset = 126;
                cooldownTimer = 600;
                SoundEngine.PlaySound(SoundID.Clown with { Volume = 0.1f } with { PitchRange = (0.4f, 1f) });
                timer--;
            }
            else {
                frameYOffset = 0;
                if (cooldownTimer > 0) cooldownTimer--;
                //Main.NewText("CDTimer" + cooldownTimer, Microsoft.Xna.Framework.Color.White);
                //Main.NewText("Timer" + timer, Microsoft.Xna.Framework.Color.White);

            }
        }
    }
    public override void NearbyEffects(int i, int j, bool closer) {
        // IIRC "closer" should correspond to whether the tile is on screen or not
        if(closer) isOnScreen = true;
        else isOnScreen = false;
    }
}


    internal class SucsGoodmanItem : ModItem {
    public override string Texture => Assets.Images.Paintings.SucsGoodman.KEY;
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;

        (Item.width, Item.height) = (34, 52);
        Item.sellPrice(0,2,0,0);
        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
        Item.createTile = ModContent.TileType<SucsGoodmanTile>();
        }
    }