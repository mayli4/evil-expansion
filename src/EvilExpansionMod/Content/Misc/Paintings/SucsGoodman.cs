
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

            // Can't use this since texture is vertical
            // AnimationFrameHeight = 56;
        }

    // Our textures animation frames are arranged horizontally, which isn't typical, so here we specify animationFrameWidth which we use later in AnimateIndividualTile

// TODO: It's better to have an actual class for this example, instead of comments

// Below is an example completely manually drawing a tile. It shows some interesting concepts that may be useful for more advanced things
/*public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
    // Instead of SetSpriteEffects
    // Flips the sprite if x coord is odd. Makes the tile more interesting
    SpriteEffects effects = SpriteEffects.None;
    if (i % 2 == 1)
        effects = SpriteEffects.FlipHorizontally;

    // Instead of AnimateIndividualTile
    // Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
    int uniqueAnimationFrame = Main.tileFrame[Type] + i % 6;
    if (i % 2 == 0)
        uniqueAnimationFrame += 3;
    if (i % 3 == 0)
        uniqueAnimationFrame += 3;
    if (i % 4 == 0)
        uniqueAnimationFrame += 3;
    uniqueAnimationFrame %= 6;

    int frameXOffset = uniqueAnimationFrame * animationFrameWidth;


    Tile tile = Main.tile[i, j];
    Texture2D texture = TextureAssets.Tile[Type].Value;

    // If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
    // The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting mode
    // While at 100% world zoom, press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
    Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

    Main.spriteBatch.Draw(
        texture,
        new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
        new Rectangle(tile.frameX + frameXOffset, tile.frameY, 16, 16),
        Lighting.GetColor(i, j), 0f, default, 1f, effects, 0f);

    return false; // return false to stop vanilla draw
}


 */

/*
    public virtual void PlaySound() {
        SoundEngine.PlaySound(SoundID.Clown with { Volume = 5f } with { PitchRange = (0.4f, 1f) });
    }
    int timer = 0;
    public override void AnimateTile(ref int frame, ref int frameCounter) {
        if(Main.rand.NextBool(50)){
            timer = 20;
        }
        if (timer != 0) 
        {
            frame = 1;
            PlaySound();
            timer -= 1;
        }
        else
        {
            frame = 0;
        }
    }
*/
}


    internal class SucsGoodmanItem : ModItem {
    public override string Texture => Assets.Images.Paintings.SucsGoodman.KEY;
    public override void SetDefaults() {
        (Item.width, Item.height) = (34, 52);
        Item.sellPrice(0,2,0,0);
        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
        Item.createTile = ModContent.TileType<SucsGoodmanTile>();
        }
    }