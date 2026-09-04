using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class SpiritFireball : ModProjectile {
    public override string Texture => Helper.PlaceholderTextureKey;

    Vector2[] _trailPositions;
    public static readonly float Gravity = 0.3f;
    public static readonly int MaxTimeLeft = 130;

    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);
    
    public readonly static Color GhostColor1 = new(214, 237, 5);
    public readonly static Color GhostColor2 = new(181, 200, 4);

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;

        Projectile.aiStyle = -1;
    }

    public override void AI() {
        _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 6)];
        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = Projectile.Center + Projectile.velocity;

        Projectile.velocity.Y += Gravity;

        if(Main.rand.NextBool(10)) { 
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.CursedTorch,
                newColor: Main.rand.NextFromList(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2)
            );

            var dir = Projectile.velocity != Vector2.Zero 
                ? -Vector2.Normalize(Projectile.velocity) 
                : -Vector2.UnitY;

            float coneAngle = Main.rand.NextFloat(-0.3f, 0.3f); 
            float backwardSpeed = Main.rand.NextFloat(0.5f, 2f);

            var spawnPosition = Projectile.Center 
                                    - (Projectile.velocity * 0.5f) 
                                    + Main.rand.NextVector2Circular(4f, 4f);

            var flame = DustFlameParticle.RequestNew(
                spawnPosition, 
                dir.RotatedBy(coneAngle) * backwardSpeed, 
                GhostColor1, 
                GhostColor1, 
                Main.rand.NextFloat(0.8f, 1.4f), 
                Main.rand.Next(18, 28)
            );

            flame.LossPerFrame = 0.12f; 
            flame.Swirly = false; 
            flame.ApplyLighting = false;

            ParticleEngine.PARTICLES.Add(flame);
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var blinker = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * Projectile.whoAmI) +
            MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var bigGlowColor = CursedSpiritNPC.GhostColor2 * (0.25f + 0.25f * blinker);
        var smallGlowColor = CursedSpiritNPC.GhostColor1;

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition - Projectile.velocity * 0.6f,
            null,
            bigGlowColor,
            0f,
            glowTexture.Size() * 0.5f,
            0.3f * Scale,
            SpriteEffects.None,
            0
        );
        Main.spriteBatch.EndBegin(snapshot);

        var trailEffect = Assets.Shaders.Trail.CursedSpiritFire.Asset.Value;
        Graphics.BeginPixelated()
            .SetEffectParams(
                trailEffect,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 34.432f),
                ("mat", Graphics.WorldTransformMatrix),
                ("stepY", 0.25f),
                ("scale", 0.25f),
                ("texture1", Assets.Images.Sample.Pebbles.Asset.Value),
                ("texture2", Assets.Images.Sample.Noise3.Asset.Value)
            )
            .DrawTrail(
                _trailPositions,
                static _ => 18f,
                static t => Color.Lerp(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2, t + 0.7f),
                trailEffect
            )
            .DrawTexture(new()
            {
                Texture = Assets.Images.Misc.Circle.Asset.Value,
                Position = Projectile.Center - Main.screenPosition,
                Color = smallGlowColor,
                Origin = 16f * Vector2.One,
                Scale = Vector2.One * 0.3f,
            })
            .ApplyOutline(CursedSpiritNPC.GhostColor1)
            .End();

        return false;
    }
}