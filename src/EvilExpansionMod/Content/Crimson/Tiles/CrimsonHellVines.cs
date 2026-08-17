using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Crimson;

public class CrimsonHellVines : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.CrimsonHellVines.KEY;

    public override void SetStaticDefaults() {
        Main.tileBlockLight[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.IsVine[Type] = true;
        TileID.Sets.VineThreads[Type] = true;
        TileID.Sets.ReplaceTileBreakDown[Type] = true;

        HitSound = SoundID.Grass;
        DustType = DustID.Crimson;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [Type];

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<CrimsonAshGrass>()];

        DustType = DustID.Corruption;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(132, 38, 51));

        TileLoader.RegisterConversion(TileID.AshVines, BiomeConversionID.Crimson, ConvertToCorruption);
    }

    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.AshVines);
                return;
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<UnderworldCorruptVines>());
                return;

        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Main.instance.TilesRenderer.CrawlToTopOfVineAndAddSpecialPoint(j, i);
        return false;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -2;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if(i % 2 == 0) {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}

//from examplemod
internal class CrimsonHellVinesGlobalTile : GlobalTile {
    private int _crimsonHellVine;
    private int _crimsonAshGrass;

    public override void SetStaticDefaults() {
        _crimsonHellVine = ModContent.TileType<CrimsonHellVines>();
        _crimsonAshGrass = ModContent.TileType<CrimsonAshGrass>();
    }

    // Random growth behavior:
    public override void RandomUpdate(int i, int j, int type) {
        if(j >= Main.worldSurface - 1) {
            return; // ExampleVine only grows above ground
        }

        Tile tile = Main.tile[i, j];
        if(!tile.HasUnactuatedTile) {
            return; // Don't grow on actuated tiles.
        }

        // Vine tiles usually grow on themselves (from the tip) or on any tile they spawn from (grass tiles usually). GrowMoreVines checks that the nearby area isn't already full of vines.
        if((tile.TileType == _crimsonHellVine || tile.TileType == _crimsonAshGrass) && WorldGen.GrowMoreVines(i, j)) {
            int growChance = 70;
            if(tile.TileType == _crimsonHellVine) {
                growChance = 7; // 10 times more likely to extend an existing vine than start a new vine
            }

            int below = j + 1;
            Tile tileBelow = Main.tile[i, below];
            if(WorldGen.genRand.NextBool(growChance) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava) {
                // We check that the vine can grow longer and is not already broken.
                bool vineIsHangingOffValidTile = false;
                for(int above = j; above > j - 10; above--) {
                    Tile tileAbove = Main.tile[i, above];
                    if(tileAbove.BottomSlope) {
                        return;
                    }

                    if(tileAbove.HasTile && tileAbove.TileType == _crimsonAshGrass && !tileAbove.BottomSlope) {
                        vineIsHangingOffValidTile = true;
                        break;
                    }
                }

                if(vineIsHangingOffValidTile) {
                    // If all the checks succeed, place the tile, copy paint from the tile we grew from, and sync the tile change.
                    tileBelow.TileType = (ushort)_crimsonHellVine;
                    tileBelow.HasTile = true;
                    tileBelow.CopyPaintAndCoating(tile);
                    WorldGen.SquareTileFrame(i, below);
                    if(Main.netMode == NetmodeID.Server) {
                        NetMessage.SendTileSquare(-1, i, below);
                    }
                }
            }
        }
    }

    // Transforming vines to ExampleVine if necessary behavior
    public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
        // This code handles transforming any vine to ExampleVine if the anchored tile happens to change to ExampleBlock. This can happen with spreading grass tiles or Clentaminator solutions. Without this code the vine would just break in those situations. 
        if(!TileID.Sets.IsVine[type]) {
            return true;
        }

        Tile tile = Main.tile[i, j];
        Tile tileAbove = Main.tile[i, j - 1];

        // We determine the tile type of the tile above this tile. If the tile doesn't exist, is actuated, or has a slopped bottom, the vine will be destroyed (-1).
        int aboveTileType = tileAbove.HasUnactuatedTile && !tileAbove.BottomSlope ? tileAbove.TileType : -1;

        // If this tile isn't the same as the one above, we need to verify that the above tile is valid.
        if(type != aboveTileType) {
            // If the above tile is a valid ExampleVine anchor, but this tile isn't ExampleVine, we change this tile into ExampleVine.
            if((aboveTileType == _crimsonAshGrass || aboveTileType == _crimsonHellVine) && type != _crimsonHellVine) {
                tile.TileType = (ushort)_crimsonHellVine;
                WorldGen.SquareTileFrame(i, j);
                return true;
            }

            // Finally, we need to handle the case where there is not longer a valid placement for ExampleVine.
            // Due to the ordering of hooks with respect to vanilla code, it is not easy to do this in a mod-compatible manner directly. Vanilla vine code or vine code from other mods might convert the vine to a new tile type, but we can't know that here.
            // If the anchor tile is invalid, we kill the tile, otherwise we change the vine tile to TileID.Vines and let the vanilla code that will run after this handle the remaining logic.
            if(type == _crimsonHellVine && aboveTileType != _crimsonAshGrass) {
                if(aboveTileType == -1) {
                    WorldGen.KillTile(i, j);
                }
                else {
                    tile.TileType = TileID.Vines;
                }
            }
        }

        return true;
    }
}