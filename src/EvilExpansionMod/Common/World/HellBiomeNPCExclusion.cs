using EvilExpansionMod.Content.Biomes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.World;

internal sealed class HellBiomeExclusionSystem : GlobalNPC {
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        foreach(var player in Main.ActivePlayers) {
            if(player.InModBiome<UnderworldCorruptionBiome>()) {
                if (pool.ContainsKey(0)) {
                    pool[0] = 0f;
                }
            }
            if(player.InModBiome<UnderworldCrimsonBiome>()) {
                if (pool.ContainsKey(0)) {
                    pool[0] = 0f;
                }
            }
        }
    }
}