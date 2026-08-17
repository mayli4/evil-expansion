using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;

namespace EvilExpansionMod.Content.Particles;

public sealed class GlowEmberParticle : BaseParticle<GlowEmberParticle> {
    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
    public Color ColorTint;
    public Color ColorShine;

    private float lossPerSecond;

    public float LossPerSecond {
        get => lossPerSecond;
        set => lossPerSecond = Math.Clamp(value, 0.002f, 1f);
    }
    
    public float LifeTime;

    public Vector2 Gravity;
    public float Randomness;

    public bool EmitLight;

    public static GlowEmberParticle NewParticle(Vector2 position, Vector2 velocity, float scale, Color tintColor, Color shineColor) {
        var particle = Pool.RequestParticle();
        particle.Position = position;
        particle.Velocity = velocity;
        particle.Scale = scale;
        particle.ColorTint = tintColor;
        particle.ColorShine = shineColor;

        return particle;
    }

    public override void FetchFromPool() {
        base.FetchFromPool();
        LifeTime = 0;
        LossPerSecond = 0.02f;
        Randomness = 0.5f;
        Gravity = -Vector2.UnitY * 0.4f;
        EmitLight = true;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        Velocity += (Gravity + Main.rand.NextVector2Circular(1, 1)) * Randomness;
        Velocity *= 1f - LossPerSecond - Randomness * 0.015f;
        Gravity *= 0.95f;

        Scale *= 1f - LossPerSecond;

        if (Scale < 0.1f)
            ShouldBeRemovedFromRenderer = true;

        LifeTime++;
        Position += Velocity;

        if (EmitLight)
            Lighting.AddLight(Position, ColorTint.ToVector3() * Utils.Remap(Scale, 0f, 0.5f, 0f, 0.4f));
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        var texture = Assets.Images.Particles.GlowEmberParticle.Asset;
        var origin = new Vector2(texture.Width() / 2, texture.Height() / 2 - 2f);

        float speed = MathF.Sqrt(Velocity.Length());
        var drawScale = new Vector2(1.1f - speed * 0.1f, 1f + speed * 0.3f) * Scale * Utils.Remap(LifeTime, -1f, 4f, 0f, 1f) * Utils.Remap(Scale, -0.5f, 0.2f, 0f, 1f);
        var shineScale = new Vector2(0.6f, 0.4f) * drawScale;

        var fadeOut = Utils.Remap(Scale, 0.1f, 0.3f, 0f, 1f);
        var fadeOutShine = Utils.Remap(Scale, 0.3f, 0.4f, 0f, 1f);
        var rotation = Velocity.ToRotation() + MathHelper.PiOver2;

        spritebatch.Draw(texture.Value, Position + settings.AnchorPosition, null, ColorTint * fadeOut, rotation, origin, drawScale, 0, 0);
        spritebatch.Draw(texture.Value, Position + settings.AnchorPosition, null, ColorShine * fadeOutShine, rotation, origin, shineScale, 0, 0);
    }
}