using EvilExpansionMod.Content.Tiles.Corruption;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Tiles;

public sealed class ConversionGlobalTile : GlobalTile {
    public override void NearbyEffects(int i, int j, int type, bool closer) {
        if (type == TileID.TreeAsh) {
            WorldGen.GetTreeBottom(i, j, out var x, out var y);
            Tile tilebelow = Main.tile[x, y + 1];
            Tile tilecurrent = Main.tile[x, y];
            if (tilebelow.TileType == ModContent.TileType<OvergrownCorruptAsh>() 
                || tilebelow.TileType == ModContent.TileType<OvergrownCorruptAsh>() 
                || tilebelow.TileType == ModContent.TileType<HellEbontree>() 
                || tilecurrent.TileType == ModContent.TileType<OvergrownCorruptAsh>() 
                || tilecurrent.TileType == ModContent.TileType<OvergrownCorruptAsh>() 
                || tilecurrent.TileType == ModContent.TileType<HellEbontree>()) 
            {
                Main.tile[i, j].TileType = (ushort)ModContent.TileType<HellEbontree>();
            }
        }
    }
}