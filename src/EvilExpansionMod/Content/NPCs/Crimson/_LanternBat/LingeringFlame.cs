using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class LingeringFlame : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 15;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60 * 3;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        if (Projectile.alpha > 0) {
            Projectile.alpha -= 5;
            if (Projectile.alpha < 0) Projectile.alpha = 0;
        }

        if (Main.rand.NextBool(3)) {
            Dust.NewDust(
                Projectile.position, Projectile.width, Projectile.height,
                DustID.Torch,
                Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f),
                100, default, Main.rand.NextFloat(1f, 1.5f)
            );
        }

        if (Projectile.timeLeft < 30) {
            Projectile.alpha += 8;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.OnFire, 180);
    }
}