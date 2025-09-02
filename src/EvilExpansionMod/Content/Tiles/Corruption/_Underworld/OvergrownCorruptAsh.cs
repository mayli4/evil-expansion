using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles.Corruption;

public class OvergrownCorruptAsh : ModTile {
    public override string Texture => Assets.Assets.Textures.Tiles.Corruption.KEY_OvergrownCorruptAshTile;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBlendAll[Type] = true;

        this.Merge(ModContent.TileType<CorruptAsh>(), TileID.Grass);
        TileID.Sets.Grass[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<CorruptAsh>();
        TileID.Sets.NeedsGrassFraming[Type] = true;

        Main.tileMerge[Type][TileID.Ash] = true;
        Main.tileMerge[TileID.Ash][Type] = true;
        Main.tileMerge[Type][TileID.ObsidianBrick] = true;
        Main.tileMerge[TileID.ObsidianBrick][Type] = true;
        Main.tileMerge[Type][TileID.HellstoneBrick] = true;
        Main.tileMerge[TileID.HellstoneBrick][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;

        DustType = DustID.Corruption;

        AddMapEntry(new Color(69, 68, 114));

        TileLoader.RegisterConversion(TileID.AshGrass, BiomeConversionID.Corruption, ConvertToCorruption);
        RegisterItemDrop(ModContent.ItemType<CorruptAshItem>());
    }

    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.AshGrass);
                return;
            case BiomeConversionID.Sand:
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<OvergrownCorruptAsh>());
                return;

        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Corruption);

        if(Helper.Spread(i, j, Type, 2, ModContent.TileType<CorruptAsh>()))
            NetMessage.SendTileSquare(-1, i, j, 3); // try spread grass

        GrowTiles(i, j);
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttempt(-2, TileID.Ash, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }

    protected virtual void GrowTiles(int i, int j) {
        var tile = Framing.GetTileSafely(i, j);
        var tileAbove = Framing.GetTileSafely(i, j - 1);
        
        //try place foliage
        if(WorldGen.genRand.NextBool(10) && !tileAbove.HasTile && tileAbove.LiquidAmount < 80) {
            if(!tile.BottomSlope && !tile.TopSlope && !tile.IsHalfBlock && !tile.TopSlope) {
                tileAbove.TileType = (ushort)ModContent.TileType<CorruptFoliage>();
                tileAbove.HasTile = true;
                tileAbove.TileFrameY = 0;
                tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(8) * 18);
                WorldGen.SquareTileFrame(i, j + 1, true);
                if(Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j - 1, 3, TileChangeType.None);
            }
        }
        
        // if(tile.BottomSlope || tile.TopSlope || tile.IsHalfBlock) {
        //     return;
        // }
        // int rubbleSpawnX = i - 1;
        // int rubbleSpawnY = j - 1;
        //
        // int rubbleWidth = 3;
        //
        // bool hasSolidGroundBelow = true;
        // for (int xCheck = 0; xCheck < rubbleWidth; xCheck++) {
        //     Tile tileBelow = Framing.GetTileSafely(rubbleSpawnX + xCheck, j + 1);
        //     if (!WorldGen.SolidTile(rubbleSpawnX + xCheck, j + 1) || tileBelow.IsHalfBlock || tileBelow.TopSlope) {
        //         hasSolidGroundBelow = false;
        //         break;
        //     }
        // }
        //
        // if (hasSolidGroundBelow && WorldGen.genRand.NextBool(1)) {
        //     WorldGen.PlaceObject(rubbleSpawnX, rubbleSpawnY, ModContent.TileType<CorruptionAshRubble>(), false, WorldGen.genRand.Next(3));
        // }
    }
}