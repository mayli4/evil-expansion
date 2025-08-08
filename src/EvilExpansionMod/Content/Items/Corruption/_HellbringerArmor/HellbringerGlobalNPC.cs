using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption._HellbringerArmor;

public class HellbringerGlobalNPC : GlobalNPC {
    public override void OnKill(NPC npc) {
        if(npc.boss) return;
        for(var i = 0; i < Main.maxPlayers; i++) {
            Player player = Main.player[i];
            if(
                player is null
                || !player.active
                || player.armor[0].type != ModContent.ItemType<HellbringerHead>()
                || player.armor[1].type != ModContent.ItemType<HellbringerBody>()
                || player.armor[2].type != ModContent.ItemType<HellbringerLegs>()
                || player.Center.DistanceSQ(npc.Center) > HellbringerHead.ShadowOrbSpawnRange * HellbringerHead.ShadowOrbSpawnRange
            ) continue;

            if(Main.rand.NextFloat() < HellbringerHead.ShadowOrbSpawnChance) {
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    npc.Center,
                    Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 8f),
                    ModContent.ProjectileType<ShadowOrbProjectile>(),
                    HellbringerHead.CorruptlingDamage,
                    0.5f,
                    player.whoAmI
                );
            }
        }
    }
}
