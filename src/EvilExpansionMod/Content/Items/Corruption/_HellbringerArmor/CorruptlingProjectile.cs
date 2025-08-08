using EvilExpansionMod.Utilities._Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption._HellbringerArmor;
public class CorruptlingProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_CorruptlingNPC;

    static float AttackRadius = 2000;
    int TypeIndex => (int)Projectile.ai[0];

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 420;
        Projectile.penetrate = 6;
        Projectile.aiStyle = -1;

        Projectile.SetAISlotNPC(1, null);
    }

    public override void AI() {
        var target = Projectile.GetAISlotNPC(1);
        Vector2 directionToTarget;
        if(!Projectile.MinionValidTarget(target, AttackRadius, false, true)) {
            if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.MinionTryGetTarget(AttackRadius, false, true, out target)) {
                Projectile.SetAISlotNPC(1, target);
                Projectile.netUpdate = true;
            }

            var owner = Main.player[Projectile.owner];
            directionToTarget = (owner.Center - (Projectile.Center + ((float)Main.GameUpdateCount * 0.05f).ToRotationVector2() * 50))
                .SafeNormalize(Vector2.Zero);
        }
        else {
            directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
        }

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToTarget * 10f, 0.02f);
        Projectile.rotation += Math.Min(Projectile.velocity.X * 0.05f, 0.15f);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Projectile.velocity = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(MathF.PI) * 8f;
        if(Projectile.MinionTryGetTarget(AttackRadius, false, true, out target)) {
            Projectile.SetAISlotNPC(1, target);
        }
    }

    public override bool PreKill(int timeLeft) {
        if(Main.dedServ) return true;

        var rotation = Main.rand.NextFloat();
        for(int i = 0; i < 3; i++) {
            var direction = rotation.ToRotationVector2();
            Gore.NewGoreDirect(
                Projectile.GetSource_Death(),
                Projectile.Center + direction * 10f - new Vector2(8, 8),
                direction * Main.rand.NextFloat(3f, 4f) + Projectile.velocity,
                Mod.Find<ModGore>("CorruptlingGore" + i).Type
            );

            rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
        }

        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var cellWidth = 40;
        var source = new Rectangle(TypeIndex * cellWidth, 0, cellWidth, texture.Height);

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            lightColor,
            Projectile.rotation,
            new Vector2(cellWidth, texture.Height) * 0.5f,
            Projectile.scale,
            Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0f
        );

        return false;
    }
}
