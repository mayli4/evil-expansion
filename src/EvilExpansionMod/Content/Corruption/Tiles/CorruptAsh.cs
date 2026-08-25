using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CorruptAsh : ModTile {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        TileID.Sets.BlockMergesWithMergeAllBlockOverride[Type] = true;
        Main.tileMerge[Type][TileID.Ash] = true;
        Main.tileMerge[TileID.Ash][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CrimsonAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CrimsonAsh>()][Type] = true;
        Main.tileMerge[Type][TileID.ObsidianBrick] = true;
        Main.tileMerge[TileID.ObsidianBrick][Type] = true;
        Main.tileMerge[Type][TileID.HellstoneBrick] = true;
        Main.tileMerge[TileID.HellstoneBrick][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<PolypOreTile>()] = true;
        Main.tileMerge[ModContent.TileType<PolypOreTile>()][Type] = true;

        DustType = DustID.Corruption;

        AddMapEntry(new Color(53, 37, 62));
        Main.tileMerge[Type][ModContent.TileType<OvergrownCorruptAsh>()] = true;

        TileLoader.RegisterConversion(TileID.Ash, BiomeConversionID.Corruption, ConvertToCorruption);
    }

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }

    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        WorldGen.ConvertTile(i, j, Type);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Chlorophyte:
            case BiomeConversionID.Purity:
                WorldGen.ConvertTile(i, j, TileID.Ash);
                return;
            case BiomeConversionID.Sand:
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptAsh>());
                return;
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonAsh>());
                return;

        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Corruption);
    }
}

public class CorruptAshItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Tiles.CorruptAshItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CorruptAsh>());
        Item.width = 16;
        Item.height = 16;
        Item.value = 5;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
    }
}