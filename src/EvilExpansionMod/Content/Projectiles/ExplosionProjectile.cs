using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Projectiles;
public class ExplosionProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";
    public static void New(
        IEntitySource source,
        Vector2 position,
        int damage,
        float knockback = 10f,
        int size = 50,
        int timeLeft = 120
    ) {
        var explosion = Projectile.NewProjectileDirect(
            source,
            position,
            Vector2.Zero,
            ModContent.ProjectileType<ExplosionProjectile>(),
            damage,
            knockback
        ).ModProjectile as ExplosionProjectile;

        explosion.Projectile.timeLeft = timeLeft;
        explosion.Projectile.width = explosion.Projectile.height = size;
        explosion.Projectile.Center = position;
        explosion.Projectile.rotation = Main.rand.NextFloatDirection() * 14f;
        explosion.Projectile.netUpdate = true;
    }

    private int _maxTimeLeft = -1;

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.hide = true;
    }

    public override void AI() {
        if(_maxTimeLeft == -1) {
            Projectile.rotation = Main.rand.NextFloatDirection();
            _maxTimeLeft = Projectile.timeLeft;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(_maxTimeLeft - Projectile.timeLeft > 1) return false;
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override bool ShouldUpdatePosition() => false;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        overWiresUI.Add(index);
    }

    public override bool PreDraw(ref Color lightColor) {
        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
        var flareTexture = Assets.Assets.Textures.Sample.Flare1.Value;
        var smokeTexture = Assets.Assets.Textures.Sample.SmokeGlow.Value;
        var noiseTexture1 = Assets.Assets.Textures.Sample.PerlinNoise.Value;
        var noiseTexture2 = Assets.Assets.Textures.Sample.Noise2.Value;

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        var progress = 1f - (float)Projectile.timeLeft / _maxTimeLeft;
        var flashScale = MathF.Pow(progress - 1f, 4);
        var flashAlpha = MathF.Pow(1f - progress, 2);

        var masterScale = Projectile.width * 0.0075f;
        flashScale *= masterScale;

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.Yellow * flashAlpha,
            0f,
            glowTexture.Size() / 2f,
            flashScale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            flareTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.Yellow * flashAlpha,
            Projectile.rotation,
            flareTexture.Size() / 2f,
            4f * flashScale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}

public class TestPlayer : ModPlayer {
    public override void ProcessTriggers(TriggersSet triggersSet) {
        if(Main.LocalPlayer.justJumped) {
            ExplosionProjectile.New(
                null,
                Main.MouseWorld,
                250,
                timeLeft: 80
            );
        }
    }
}
