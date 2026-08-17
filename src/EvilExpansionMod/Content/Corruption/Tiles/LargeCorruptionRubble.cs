using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Corruption;

//dude

public class LargeCorruptionRubble : ModTile {

    public override string Texture => Assets.Images.Corruption.Tiles.Jungle.LargeCorruptionRubble.KEY;

    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = false;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.Origin = new(2, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
        TileObjectData.newTile.AnchorValidTiles = [TileID.CorruptJungleGrass, TileID.JungleGrass];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 9;

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(109, 106, 188));
        DustType = DustID.CorruptPlants;
        TileLoader.RegisterConversion(TileID.PlantDetritus, BiomeConversionID.Corruption, ConvertToCorruption);
    }

    public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
        int tile = Main.tile[i, j].TileType;

        if(Framing.GetTileSafely(i, j + 1).TileType == tile)
            return false;

        Helper.GetTopLeftTile(ref i, ref j);
        Helper.ConvertTiles(i, j, 3, 2, TileID.PlantDetritus);
        return false;
    }

    public override void Convert(int i, int j, int conversionType) {
        switch(conversionType) {
            case BiomeConversionID.Purity:
                int type = Main.tile[i, j].TileType;

                if(Framing.GetTileSafely(i, j + 1).TileType == type)
                    return;

                Helper.GetTopLeftTile(ref i, ref j);
                Helper.ConvertTiles(i, j, 3, 2, TileID.PlantDetritus);
                return;
            case BiomeConversionID.Corruption:
                int corruptType = Main.tile[i, j].TileType;
                Helper.GetTopLeftTile(ref i, ref j);

                if(Framing.GetTileSafely(i, j + 2).TileType == corruptType)
                    return;

                Helper.ConvertTiles(i, j, 3, 2, ModContent.TileType<LargeCorruptionRubble>());
                return;

        }
    }
}