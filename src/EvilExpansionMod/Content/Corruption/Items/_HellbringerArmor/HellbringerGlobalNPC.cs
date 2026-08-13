using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption._HellbringerArmor;

public class HellbringerGlobalNPC : GlobalNPC {

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
        OnHit(npc);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
        OnHit(npc);
    }

    static void OnHit(NPC npc) {
        if(npc.friendly) return;
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
                    npc.Center.DirectionTo(player.Center).RotatedByRandom(MathHelper.PiOver4 * 0.25f) * 14f,
                    ModContent.ProjectileType<ShadowOrbProjectile>(),
                    HellbringerHead.CorruptlingDamage,
                    0.5f,
                    player.whoAmI
                );
            }
        }
    }
}
