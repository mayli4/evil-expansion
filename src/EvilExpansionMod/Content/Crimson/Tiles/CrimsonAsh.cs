using EvilExpansionMod.Content.Corruption;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class CrimsonAsh : ModTile {
    public override string Texture => Assets.Images.Crimson.Tiles.CrimsonAshTile.KEY;

    public override void SetStaticDefaults() {
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;

        TileID.Sets.BlockMergesWithMergeAllBlockOverride[Type] = true;
        Main.tileMerge[Type][TileID.Ash] = true;
        Main.tileMerge[TileID.Ash][Type] = true;
        Main.tileMerge[Type][ModContent.TileType<CorruptAsh>()] = true;
        Main.tileMerge[ModContent.TileType<CorruptAsh>()][Type] = true;
        Main.tileMerge[Type][TileID.ObsidianBrick] = true;
        Main.tileMerge[TileID.ObsidianBrick][Type] = true;
        Main.tileMerge[Type][TileID.HellstoneBrick] = true;
        Main.tileMerge[TileID.HellstoneBrick][Type] = true;

        Main.tileMerge[Type][ModContent.TileType<CartilageOreTile>()] = true;
        Main.tileMerge[ModContent.TileType<CartilageOreTile>()][Type] = true;

        DustType = DustID.Crimson;

        AddMapEntry(new Color(107, 66, 63));
        Main.tileMerge[Type][ModContent.TileType<CrimsonAshGrass>()] = true;

        TileLoader.RegisterConversion(TileID.Ash, BiomeConversionID.Crimson, ConvertToCrimson);
    }

    public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
        sightColor = Color.Yellow;
        return true;
    }

    public bool ConvertToCrimson(int i, int j, int type, int conversionType) {
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
            case BiomeConversionID.Crimson:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonAsh>());
                return;
            case BiomeConversionID.Corruption:
                WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptAsh>());
                return;

        }
    }

    public override void RandomUpdate(int i, int j) {
        WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Crimson);
    }
}

public class CrimsonAshItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Tiles.CrimsonAshItem.KEY;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<CrimsonAsh>());
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