using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Crimson;

public class CrimsonFoliage : ModTile {
    public override string Texture => Assets.Textures.Tiles.Crimson.CrimsonAshFoliageSmall.KEY;

    public const int StyleRange = 6;

    public override void SetStaticDefaults() {
        const int TileHeight = 30;

        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileBlockLight[Type] = false;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = DustID.CrimsonPlants;
        HitSound = SoundID.Grass;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [TileHeight];
        TileObjectData.newTile.DrawYOffset = -(TileHeight - 18);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = StyleRange;

        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<CrimsonAshGrass>()];
        AddMapEntry(new(104, 156, 7));

        TileObjectData.addTile(Type);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;
    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if(Main.player[Player.FindClosest(new Vector2(i, j).ToWorldCoordinates(0, 0), 16, 16)].HeldItem.type == ItemID.Sickle)
            yield return new Item(ItemID.Hay, Main.rand.Next(1, 3));

        if(Main.player[Player.FindClosest(new Vector2(i, j).ToWorldCoordinates(0, 0), 16, 16)].HasItem(ItemID.Blowpipe))
            yield return new Item(ItemID.Seed, Main.rand.Next(1, 3));
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        var tileBelow = Framing.GetTileSafely(i, j + 1);
        int type = -1;
        if(tileBelow.HasTile && !tileBelow.BottomSlope) {
            type = tileBelow.TileType;
        }
        if(type == ModContent.TileType<CrimsonAshGrass>() || type == Type) {
            return true;
        }
        WorldGen.KillTile(i, j);
        return true;
    }
}