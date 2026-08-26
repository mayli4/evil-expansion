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

namespace EvilExpansionMod.Content.Crimson;

public class LingeringFlameProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_0";

    public NPC ParentNPC => Main.npc[(int)Projectile.ai[0]];

    static int MAX_LIFETIME = 160;

    int _freePositionCount;
    Vector2[] _trailPositions = null!;
    readonly Vector2[] _trailVelocities = new Vector2[8];

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 1;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MAX_LIFETIME;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override bool ShouldUpdatePosition() => false;
    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        base.OnHitPlayer(target, info);
        target.AddBuff(BuffID.OnFire, 300, false);
    }
    public override void AI() {
        Projectile.direction = (int)Projectile.ai[1];
        if(_freePositionCount < _trailVelocities.Length) {
            Projectile.Center = ParentNPC.Center + Vector2.UnitY * 30f;

            _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, _trailVelocities.Length)];
            if(_freePositionCount == 0 || Projectile.timeLeft % 8 == 0) {
                _freePositionCount++;
            }

            for(var i = _freePositionCount; i < _trailPositions.Length; i++) {
                _trailPositions[i] = Projectile.Center;
                _trailVelocities[i] = Vector2.Zero;
            }
        }

        for(var i = _freePositionCount - 1; i >= 0; i--) {
            _trailPositions[i] += _trailVelocities[i];
            _trailVelocities[i] += Vector2.UnitY * 0.075f * (MathF.Sin(Main.GameUpdateCount * 0.15f + i + Projectile.timeLeft * 0.05f) * 0.15f + 1f);

            Lighting.AddLight(_trailPositions[i], TorchID.Orange);
        }

        if(Projectile.timeLeft > MAX_LIFETIME - 60 && Projectile.timeLeft % Main.rand.Next(4, 6) == 0) {
            var position = _trailPositions[_freePositionCount - 1] + Vector2.UnitY * 20f;
            var direction = Projectile.direction * -Vector2.UnitX;

            var ember = GlowEmberParticle.NewParticle(
                position + Main.rand.NextVector2Unit() * 20f,
                direction * Main.rand.NextFloat(4.2f, 7.5f),
                Main.rand.NextFloat(0.25f, 1.5f),
                Color.Orange with { A = 0 },
                Color.White with { A = 0 });

            ember.Randomness *= 2f;
            ember.LossPerSecond *= 2f;
            ParticleEngine.PARTICLES.Add(ember);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if(Projectile.timeLeft < MAX_LIFETIME - 80) return false;

        for(var i = 0; i < _trailPositions.Length - 1; i++) {
            var lineStart = _trailPositions[i];
            var lineEnd = _trailPositions[i + 1];

            float _ = 0f;
            if(Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 30f, ref _)) {
                return true;
            }
        }

        return false;
    }

    public override bool PreDraw(ref Color lightColor) {
        var flameEffect = Assets.Shaders.Trail.BatLingeringFlame.Asset.Value;

        var colorA = Color.Yellow;
        var colorB = Color.Red;

        Renderer.BeginPixelated(Graphics.WorldTransformMatrix)
            // uImage0
            .SetTexture(0, Assets.Images.Sample.Noise4.Asset.Value)
            .SetSamplerState(0, SamplerState.PointWrap)
            // uImage1
            .SetTexture(1, Assets.Images.Sample.Noise2.Asset.Value)
            .SetSamplerState(1, SamplerState.PointWrap)
            // uImage2
            .SetTexture(2, Assets.Images.Sample.BubblyNoise.Asset.Value)
            .SetSamplerState(2, SamplerState.PointWrap)
            .SetEffectParams(
                flameEffect,
                ("uTime", Main.GameUpdateCount * 0.01f),
                ("uProgress", 1f - (float)Projectile.timeLeft / MAX_LIFETIME),
                ("uDirection", Projectile.ai[1]),
                ("uColorA", colorA),
                ("uColorB", colorB))
            .DrawTrail(
                _trailPositions,
                t => 65,
                static _ => Color.White,
                flameEffect)
            .ApplyOutline(colorB)
            .ApplyBloom()
            .End();

        return false;
    }
}