using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Tiles.Corruption;

public class CorruptionAshRubble : ModTile {
    public override string Texture => Assets.Textures.Tiles.Corruption.KEY_CorruptionAshRubble;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<CorruptAsh>(), ModContent.TileType<OvergrownCorruptAsh>() };
        TileObjectData.addTile(Type);

        DustType = DustID.Corruption;

        AddMapEntry(new Color(69, 68, 114));
    }
}