using Terraria;

namespace EvilExpansionMod.Utilities;

public static class NPCExtensions {
    public static bool CanBeDamagedByPlayer(this NPC npc, Player player) {
        return !npc.friendly
               && !npc.dontTakeDamage
               && npc.active
               && npc.immune[player.whoAmI] <= 0;
    }
}