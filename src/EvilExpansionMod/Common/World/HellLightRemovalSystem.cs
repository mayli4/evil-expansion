using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Corruption;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.World;

internal sealed class HellLightRemovalSystem : ModSystem {
    public override void Load() {
        On_TileLightScanner.ApplyHellLight += TileLightScanner_ApplyHellLight;
    }

    private void TileLightScanner_ApplyHellLight(On_TileLightScanner.orig_ApplyHellLight orig, TileLightScanner self, Tile tile, int x, int y, ref Vector3 lightColor) {
        orig.Invoke(self, tile, x, y, ref lightColor);

        var underworldCorruptLightColor = new Vector3(0.6f, 0.65f, 0.08f);
        var underworldCrimsonLightColor = new Vector3(0.65f, 0.45f, 0.2f); //new Vector3(0.45f, 0.25f, 0.0f) previously

        if(Main.LocalPlayer.InModBiome<UnderworldCorruptionBiome>()) {
            if((!tile.HasTile || !Main.tileNoSunLight[tile.TileType] || // "There is no tile" OR "tile allows sunlight", OR
                ((tile.Slope != 0 || tile.IsHalfBlock) &&
                    Main.tile[x, y - 1].LiquidAmount == 0 &&
                    Main.tile[x, y + 1].LiquidAmount == 0 &&
                    Main.tile[x - 1, y].LiquidAmount == 0 &&
                    Main.tile[x + 1, y].LiquidAmount == 0)) && // Tile is not surrounded by liquids, AND
                (Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) && // Wall on the tile is transparent, AND
                tile.LiquidAmount < 200 && // Liquid in the tile is below a threshold, AND
                (!tile.IsHalfBlock || Main.tile[x, y - 1].LiquidAmount < 200)) { // "Tile is solid" OR "the liquid in tile below is below a threshold"
                    lightColor = underworldCorruptLightColor * 1f;
            }
            if((!tile.HasTile || tile.IsHalfBlock || !Main.tileNoSunLight[tile.TileType]) && tile.LiquidAmount < byte.MaxValue) { // If the tile is a half block OR allows sunlight
                lightColor = underworldCorruptLightColor * 0.5f;
            }
            if(!tile.HasTile) { // If the tile... has no tile... I suppose this is "ambient light"?
                lightColor = underworldCorruptLightColor * 0.5f;
            }
            if(!Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) { // Make it dark as fuck if there is a non-transparent wall
                lightColor = underworldCrimsonLightColor * 0f;
            }
        }

        if(Main.LocalPlayer.InModBiome<UnderworldCrimsonBiome>()) {
            if((!tile.HasTile || !Main.tileNoSunLight[tile.TileType] || // "There is no tile" OR "tile allows sunlight", OR
                ((tile.Slope != 0 || tile.IsHalfBlock) &&
                    Main.tile[x, y - 1].LiquidAmount == 0 &&
                    Main.tile[x, y + 1].LiquidAmount == 0 &&
                    Main.tile[x - 1, y].LiquidAmount == 0 &&
                    Main.tile[x + 1, y].LiquidAmount == 0)) && // Tile is not surrounded by liquids, AND
                (Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) && // Wall on the tile is transparent, AND
                tile.LiquidAmount < 200 && // Liquid in the tile is below a threshold, AND
                (!tile.IsHalfBlock || Main.tile[x, y - 1].LiquidAmount < 200)) { // "Tile is solid" OR "the liquid in tile below is below a threshold"
                lightColor = underworldCrimsonLightColor * 1f;
            }
            if((!tile.HasTile || tile.IsHalfBlock || !Main.tileNoSunLight[tile.TileType]) && tile.LiquidAmount < byte.MaxValue) {
                lightColor = underworldCrimsonLightColor * 0.5f;
            }
            if(!tile.HasTile) {
                lightColor = underworldCrimsonLightColor * 0.5f;
            }
            if(!Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) { // Make it dark as fuck if there is a non-transparent wall
                lightColor = underworldCrimsonLightColor * 0f;
            }

        }

        //if(Main.LocalPlayer.InModBiome<UnderworldCrimsonBiome>()) {
        //    if((!tile.HasTile || !Main.tileNoSunLight[tile.TileType] || ((tile.Slope != 0 || tile.IsHalfBlock) && Main.tile[x, y - 1].LiquidAmount == 0 && Main.tile[x, y + 1].LiquidAmount == 0 && Main.tile[x - 1, y].LiquidAmount == 0 && Main.tile[x + 1, y].LiquidAmount == 0)) && (Main.wallLight[tile.WallType] || tile.WallType == 73 || tile.WallType == 227) && tile.LiquidAmount < 200 && (!tile.IsHalfBlock || Main.tile[x, y - 1].LiquidAmount < 200)) {
        //        lightColor = underworldCrimsonLightColor;
        //    }
        //    if((!tile.HasTile || tile.IsHalfBlock || !Main.tileNoSunLight[tile.TileType]) && tile.LiquidAmount < byte.MaxValue) {
        //        lightColor = underworldCrimsonLightColor / 2;
        //    }

        //    if(!tile.HasTile) {
        //        lightColor = underworldCrimsonLightColor / 2;
        //    }
        //}
    }
}