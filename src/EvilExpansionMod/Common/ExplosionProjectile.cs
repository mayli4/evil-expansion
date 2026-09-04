using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.CameraModifiers;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Projectiles;

public class ExplosionProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";
    public static void New(
        IEntitySource source,
        Vector2 position,
        int damage,
        Color startColor,
        Color endColor,
        float knockback = 10f,
        int size = 50,
        int timeLeft = 120,
        bool friendly = true,
        bool hostile = false
    ) {
        _startColor = startColor;
        _endColor = endColor;

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
        explosion.Projectile.friendly = friendly;
        explosion.Projectile.hostile = hostile;
    }

    private int _maxTimeLeft = -1;

    private static Color _startColor;
    private static Color _endColor;

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.hide = true;
    }

    public override void AI() {
        if(_maxTimeLeft == -1) {
            Projectile.rotation = Main.rand.NextFloatDirection();
            _maxTimeLeft = Projectile.timeLeft;

            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.85f));
            for(int i = 0; i < 8; i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos = Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                // var newDustData = new Smoke.Data()
                // {
                //     InitialLifetime = 40,
                //     ElapsedFrames = 0,
                //     InitialOpacity = 0.8f,
                //     ColorStart = Color.Black,
                //     ColorFade = Color.Black,
                //     Spin = 0f,
                //     InitialScale = Main.rand.NextFloat(0.5f, 2f)
                // };
                //
                // var newDust = Dust.NewDustPerfect(
                //     dustPos,
                //     ModContent.DustType<Smoke>(),
                //     null,
                //     0,
                //     newColor: Color.White,
                //     newDustData.InitialScale
                // );
                //
                // newDust.customData = newDustData;
            }

            for(int i = 0; i < 20; i++) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TreasureSparkle);
            }
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
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var starTexture = Assets.Images.Sample.Star3.Asset.Value;
        var noiseTexture1 = Assets.Images.Sample.Noise1.Asset.Value;
        var noiseTexture2 = Assets.Images.Sample.Noise2.Asset.Value;
        var explosionEffect = Assets.Shaders.Pixel.Explosion.Asset.Value;

        var progress = 1f - (float)Projectile.timeLeft / _maxTimeLeft;

        var explosionProgress = 1f - MathF.Pow(progress - 1f, 2);

        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetTexture(1, noiseTexture2)
            .SetEffectParams(
                explosionEffect,
                ("time", explosionProgress + Projectile.whoAmI * 438.8239f),
                ("progress", explosionProgress),
                ("startColor", _startColor.ToVector4()),
                ("endColor", Color.Black.ToVector4())
            )
            .DrawTexture(new()
            {
                Texture = noiseTexture1,
                Position = Projectile.position,
                Size = Projectile.Size,
                Effect = explosionEffect,
            })
            .ApplyOutline(Color.Lerp(Color.DarkRed, Color.Transparent, explosionProgress))
            .SetEffectParams(
                explosionEffect,
                ("time", explosionProgress + Projectile.whoAmI * 638.8239f),
                ("progress", MathF.Pow(1f - explosionProgress, 4)),
                ("startColor", Color.Transparent.ToVector4()),
                ("endColor", _endColor.ToVector4())
            )
            .DrawTexture(new()
            {
                Texture = noiseTexture1,
                Position = Projectile.position,
                Size = Projectile.Size,
                Effect = explosionEffect,
            })
            .End();

        var flashScale = MathF.Pow(progress - 1f, 4);
        var flashAlpha = 1f - MathF.Pow(-progress * 3f, 2);

        var glowScale = Projectile.width * 0.0035f;
        flashScale *= glowScale;

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });
       
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            _startColor * flashAlpha,
            0f,
            glowTexture.Size() / 2f,
            1.5f * flashScale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            starTexture,
            Projectile.Center - Main.screenPosition
                + 0.2f * Main.rand.NextVector2Square(-Projectile.width, Projectile.width),
            null,
            _startColor * flashAlpha,
            Projectile.rotation + Main.rand.NextFloat() * 0.1f,
            glowTexture.Size() / 2f,
            1.8f * flashScale * Main.rand.NextFloat(),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Restart(ss);
        return false;
    }
}
