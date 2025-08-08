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

    int TypeIndex => (int)Projectile.ai[0];
    int _target = -1;

    public override void SetDefaults() {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 420;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
    }

    public override void AI() {
        if(!IsTargetValid(_target)) {
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                TargetClosest(2000);
                if(!IsTargetValid(_target)) {
                    Projectile.Kill();
                    return;
                }
            }

            return;
        }

        var target = Main.npc[_target];
        var directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToTarget * 10f, 0.1f);
        Projectile.rotation += MathF.Sign(Projectile.velocity.X) * 0.1f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        TargetClosest(2000);
    }

    public override bool PreKill(int timeLeft) {
        if(Main.dedServ) return true;

        var rotation = Main.rand.NextFloat();
        for(int i = 0; i < 3; i++) {
            var direction = rotation.ToRotationVector2();
            Gore.NewGoreDirect(
                Projectile.GetSource_Death(),
                Projectile.Center + direction * 10f - new Vector2(8, 8),
                direction * Main.rand.NextFloat(3f, 5f),
                Mod.Find<ModGore>("CorruptlingGore" + i).Type
            );

            rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
        }

        return true;
    }

    static bool IsTargetValid(int target) => target != -1 && Main.npc[target] != null && Main.npc[target].active;

    void TargetClosest(float maxDistance) {
        int closest = -1;
        float minDistance = float.MaxValue;

        for(var i = 0; i < Main.maxNPCs; i++) {
            if(!IsTargetValid(i)) continue;

            var npc = Main.npc[i];
            var npcDistance = npc.Center.DistanceSQ(Projectile.Center);
            if(npcDistance <= maxDistance * maxDistance && npcDistance < minDistance) {
                closest = i;
                minDistance = npcDistance;
            }
        }

        _target = closest;
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
