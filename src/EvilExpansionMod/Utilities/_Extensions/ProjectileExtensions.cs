using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace EvilExpansionMod.Utilities._Extensions;
public static class ProjectileExtensions {
    public static bool MinionValidTarget(this Projectile projectile, NPC npc, float radius, bool lineOfSight, bool originPlayer) {
        Player owner = Main.player[projectile.owner];
        Vector2 origin = originPlayer ? owner.Center : projectile.Center;

        if(npc == null || !npc.active || npc.friendly || !npc.CanBeChasedBy(projectile)) return false;
        return Vector2.DistanceSquared(origin, npc.Center) <= radius * radius
            && (!lineOfSight || Collision.CanHit(projectile.Center, 1, 1, npc.Center, 1, 1));
    }


    // modified https://github.com/ProjectStarlight/StarlightRiver/blob/ef74e8c3abd6c3226fb55d1eee71165154c3a103/Helpers/MinionTargetingHelper.cs#L16
    public static IEnumerable<NPC> MinionTargets(this Projectile projectile, float radius, bool lineOfSight, bool originPlayer) {
        Player owner = Main.player[projectile.owner];
        Vector2 origin = originPlayer ? owner.Center : projectile.Center;

        if(owner.HasMinionAttackTargetNPC) {
            NPC npc = Main.npc[owner.MinionAttackTargetNPC];
            if(Vector2.Distance(origin, npc.Center) <= radius && Collision.CanHit(projectile.Center, 1, 1, npc.Center, 1, 1)) {
                return Main.npc.Skip(owner.MinionAttackTargetNPC).Take(1);
            }
        }

        return Main.npc.Where(npc => projectile.MinionValidTarget(npc, radius, lineOfSight, originPlayer));
    }

    public static bool MinionTryGetTarget(
        this Projectile projectile,
        float radius,
        bool lineOfSight,
        bool originPlayer,
        out NPC closest
    ) {
        Player owner = Main.player[projectile.owner];
        Vector2 origin = originPlayer ? owner.Center : projectile.Center;

        closest = null;
        var minDistance = float.MaxValue;

        foreach(var npc in projectile.MinionTargets(radius, lineOfSight, originPlayer)) {
            var distance = Vector2.DistanceSquared(npc.Center, origin);
            if(distance < minDistance) {
                closest = npc;
                minDistance = distance;
            }
        }

        return closest != null;
    }

    public static void SetAISlotNPC(this Projectile projectile, int index, NPC npc) {
        projectile.ai[index] = npc?.whoAmI ?? -1;
    }

    public static NPC GetAISlotNPC(this Projectile projectile, int index) {
        var whoAmI = (int)projectile.ai[index];
        return whoAmI >= 0 && whoAmI < Main.npc.Length ? Main.npc[whoAmI] : null;
    }
}
