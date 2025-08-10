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

            Main.instance.CameraModifiers.Add(new ExplosionShakeCameraModifier(12f, 0.85f));
            for(int i = 0; i < 8; i++) {
                var randomDirection = Main.rand.NextVector2Unit();
                var dustPos = Projectile.Center + randomDirection * Main.rand.NextFloat(Projectile.width * 0.5f);

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.8f,
                    ColorStart = Color.Black,
                    ColorFade = Color.Black,
                    Spin = 0f,
                    InitialScale = Main.rand.NextFloat(0.5f, 2f)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    null,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;
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
        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
        var starTexture = Assets.Assets.Textures.Sample.Star3.Value;
        var noiseTexture1 = Assets.Assets.Textures.Sample.Noise1.Value;
        var noiseTexture2 = Assets.Assets.Textures.Sample.Noise2.Value;
        var explosionEffect = Assets.Assets.Effects.Pixel.Explosion.Value;

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });

        Main.graphics.GraphicsDevice.Textures[1] = noiseTexture2;

        var progress = 1f - (float)Projectile.timeLeft / _maxTimeLeft;

        var explosionProgress = 1f - MathF.Pow(progress - 1f, 2);
        Graphics.BeginPipeline(0.5f, new() { CustomEffect = explosionEffect })
            .EffectParams(
                explosionEffect,
                ("time", explosionProgress + Projectile.whoAmI * 438.8239f),
                ("progress", explosionProgress),
                ("startColor", Color.Yellow.ToVector4()),
                ("endColor", Color.Black.ToVector4())
            )
            .DrawSprite(
                noiseTexture1,
                new Rectangle(
                    (int)(Projectile.position.X - Main.screenPosition.X),
                    (int)(Projectile.position.Y - Main.screenPosition.Y),
                    Projectile.width,
                    Projectile.height
                )
            )
            .ApplyOutline(Color.Lerp(Color.DarkRed, Color.Transparent, explosionProgress))
            .Reset(0.5f, new() { CustomEffect = explosionEffect })
            .EffectParams(
                explosionEffect,
                ("time", explosionProgress + Projectile.whoAmI * 638.8239f),
                ("progress", MathF.Pow(1f - explosionProgress, 4)),
                ("startColor", Color.Transparent.ToVector4()),
                ("endColor", Color.LightGoldenrodYellow.ToVector4())
            )
            .DrawSprite(
                noiseTexture1,
                new Rectangle(
                    (int)(Projectile.position.X - Main.screenPosition.X),
                    (int)(Projectile.position.Y - Main.screenPosition.Y),
                    Projectile.width,
                    Projectile.height
                )
            )
            .Flush();

        var flashScale = MathF.Pow(progress - 1f, 4);
        var flashAlpha = 1f - MathF.Pow(-progress * 3f, 2);

        var glowScale = Projectile.width * 0.0035f;
        flashScale *= glowScale;

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            Color.Yellow * flashAlpha,
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
            Color.Yellow * flashAlpha,
            Projectile.rotation + Main.rand.NextFloat(),
            glowTexture.Size() / 2f,
            1.8f * flashScale * Main.rand.NextFloat(),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}
