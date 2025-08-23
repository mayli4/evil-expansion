using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Utilities;
static partial class Helper {
    public static bool Spread(int i, int j, int type, int chance, params int[] validAdjacentTypes) {
        if(Main.rand.NextBool(chance)) {
            var adjacents = OpenAdjacents(i, j, true, validAdjacentTypes);

            if(adjacents.Count == 0)
                return false;

            Point p = adjacents[Main.rand.Next(adjacents.Count)];

            Framing.GetTileSafely(p.X, p.Y).TileType = (ushort)type;
            if(Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, p.X, p.Y, 1, TileChangeType.None);
            return true;
        }
        return false;
    }

    public static List<Point> OpenAdjacents(int i, int j, bool requiresAir, params int[] types) {
        var p = new List<Point>();
        for(int k = -1; k < 2; ++k)
            for(int l = -1; l < 2; ++l)
                if(!(l == 0 && k == 0) && Framing.GetTileSafely(i + k, j + l).HasTile && types.Contains(Framing.GetTileSafely(i + k, j + l).TileType))
                    if(!requiresAir || OpenToAir(i + k, j + l))
                        p.Add(new Point(i + k, j + l));

        return p;
    }

    public static bool OpenToAir(int i, int j) {
        for(int k = -1; k < 2; ++k)
            for(int l = -1; l < 2; ++l)
                if(!(l == 0 && k == 0) && !WorldGen.SolidOrSlopedTile(i + k, j + l))
                    return true;

        return false;
    }

    public static void GetTopLeftTile(ref int i, ref int j) {
        var tile = Framing.GetTileSafely(i, j);
        var data = TileObjectData.GetTileData(tile);

        if(data is null)
            return;

        (i, j) = (i - tile.TileFrameX % data.CoordinateFullWidth / 18, j - tile.TileFrameY % data.CoordinateFullHeight / 18);
    }

    /// <summary> Tries to place or extend a vine at the given coordinates. </summary>
    /// <param name="i"> The tile's X coordinate. </param>
    /// <param name="j"> The tile's Y coordinate. </param>
    /// <param name="type"> The tile's type. </param>
    /// <param name="maxLength"> The maximum length this vine can grow. Does NOT instantly grow a vine of the given length. </param>
    /// <param name="reversed"> Whether this vine grows from the ground up. </param>
    /// <param name="sync"> Whether the tile changes should be automatically synced. </param>
    /// <returns> Whether the tile was successfully placed. </returns>
    public static bool GrowVine(int i, int j, int type, int maxLength = 15, bool reversed = false, bool sync = true) {
        if(reversed) {
            while(Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == type)
                j++; //Move to the bottom of the vine

            for(int y = 0; y < maxLength; y++) {
                if(Main.tile[i, j].HasTile && Main.tile[i, j].TileType == type)
                    j--; //Move to the next available tile above
            }
        }
        else {
            while(Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].TileType == type)
                j--; //Move to the top of the vine

            for(int y = 0; y < maxLength; y++) {
                if(Main.tile[i, j].HasTile && Main.tile[i, j].TileType == type)
                    j++; //Move to the next available tile below
            }
        }

        if(Main.tile[i, j].TileType == type)
            return false; //The tile already exists; we've hit the max length

        WorldGen.PlaceObject(i, j, type, true);

        if(Main.tile[i, j].TileType != type)
            return false; //Tile placement failed

        if(Main.netMode != NetmodeID.SinglePlayer && sync)
            NetMessage.SendTileSquare(-1, i, j, 1, 1);

        return true;
    }

    /// <summary> Based on <see cref="TileLoader.ConvertTile"/>.<br/>
    /// Converts all tiles within the specified area, then frames and sends the changes over the network. Necessary for multitiles. </summary>
    public static bool ConvertTiles(int i, int j, int width, int height, int newType, bool frameAndSend = true) {
        ushort oldType = Main.tile[i, j].TileType;

        if(oldType == newType)
            return false;

        for(int x = i; x < i + width; x++) {
            for(int y = j; y < j + height; y++) {
                var t = Main.tile[x, y];
                if(t.TileType != oldType)
                    continue;

                t.TileType = (ushort)newType;
            }
        }

        if(frameAndSend) {
            WorldGen.RangeFrame(i, j, i + width - 1, j + height - 1);

            if(Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, i, j, width, height);
        }

        return true;
    }
    
    public static void AnchorSelfTo(this ModTile tile, params int[] types) => AnchorSelfTo(tile.Type, types);

    /// <inheritdoc cref="AnchorSelfTo"/>
    public static void AnchorSelfTo(int modTileType, params int[] types)
    {
        foreach (int type in types)
        {
            if (TileObjectData.GetTileData(type, 0) is TileObjectData data && data.AnchorValidTiles != null)
                data.AnchorValidTiles = [.. data.AnchorValidTiles, modTileType];
        }
    }
    
    public static void Merge(this ModTile tile, params int[] otherIds)
    {
        foreach (int id in otherIds)
        {
            Main.tileMerge[tile.Type][id] = true;
            Main.tileMerge[id][tile.Type] = true;
        }
    }
}
