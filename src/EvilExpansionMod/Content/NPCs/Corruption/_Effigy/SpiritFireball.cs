using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

internal class SpiritFireball : ModProjectile {
    public override string Texture => "Terraria/Images/Item_0";

    Vector2[] _trailPositions;
    public static readonly float Gravity = 0.3f;
    public static readonly int MaxTimeLeft = 130;

    float Scale => 1f - MathF.Pow((float)(MaxTimeLeft - Projectile.timeLeft) / MaxTimeLeft, 2);

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
        _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 7)];
        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = Projectile.Center + Projectile.velocity;

        Projectile.velocity.Y += Gravity;

        if(Main.rand.NextBool(14)) Dust.NewDust(
            Projectile.position,
            Projectile.width,
            Projectile.height,
            DustID.CursedTorch,
            newColor: Main.rand.NextFromList(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2)
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var glowTexture = Assets.Assets.Textures.Sample.Glow1.Value;
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

        var trailEffect = Assets.Assets.Effects.Trail.CursedSpiritFire.Value;
        Renderer.BeginPipeline(RenderTarget.HalfScreen)
            .DrawTrail(
                _trailPositions,
                static _ => 14f,
                static t => Color.Lerp(CursedSpiritNPC.GhostColor1, CursedSpiritNPC.GhostColor2, t + 0.7f),
                trailEffect,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 34.432f),
                ("mat", Renderer.HalfScreenEffectMatrix),
                ("stepY", 0.25f),
                ("scale", 0.25f),
                ("texture1", Assets.Assets.Textures.Sample.Pebbles.Value),
                ("texture2", Assets.Assets.Textures.Sample.Noise2.Value)
            )
            .DrawSprite(
                Assets.Assets.Textures.Misc.Circle.Value,
                Projectile.Center - Main.screenPosition,
                color: smallGlowColor,
                origin: 16f * Vector2.One,
                scale: Vector2.One * 0.3f
            )
            .ApplyOutline(CursedSpiritNPC.GhostColor1)
            .Flush();

        return false;
    }
}