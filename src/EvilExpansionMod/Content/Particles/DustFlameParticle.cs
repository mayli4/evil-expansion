using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;

namespace EvilExpansionMod.Content.Particles;

public class DustFlameParticle : BaseParticle<DustFlameParticle>
{
    public bool ApplyLighting;
    public Color ColorInsideTint;
    public Color ColorTint;

    public int LifeTime;
    public float LossPerFrame;
    private int maxLifeTime;

    public Vector2 Position;
    public float Rotation;
    public float Scale;

    public bool Swirly;
    private int variant;
    public Vector2 Velocity;

    public static DustFlameParticle RequestNew(Vector2 position, Vector2 velocity, Color color, Color insideColor, float scale, int lifeTime = 20)
    {
        var particle = Pool.RequestParticle();
        particle.Position = position;
        particle.Velocity = velocity;
        particle.ColorTint = color;
        particle.ColorInsideTint = insideColor;
        particle.Scale = scale;
        particle.maxLifeTime = lifeTime;
        particle.variant = Main.rand.Next(6);
        particle.Rotation = (int)Math.Round(velocity.ToRotation() / MathHelper.PiOver4) * MathHelper.PiOver4 + MathHelper.PiOver2;
        particle.LossPerFrame = 0.15f;
        return particle;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Swirly = false;
        ApplyLighting = false;
        LifeTime = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;
        Velocity *= 1f - LossPerFrame;
        Velocity.Y -= 0.03f;
        Scale *= 0.99f;

        if (Collision.SolidTiles(Position - new Vector2(6), 12, 12))
        {
            LifeTime++;
            Velocity *= 0.8f;
        }

        if (++LifeTime >= maxLifeTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var progress = (float)LifeTime / maxLifeTime;

        var frameCount = Swirly ? 8 : 10;
        var texture = Swirly ? Assets.Images.Particles.SwirlyFlameParticle.Asset.Value : Assets.Images.Particles.DustFlameParticle.Asset.Value;
        var frame = texture.Frame(frameCount, 6, (int)MathF.Floor(progress * frameCount), variant % 3);
        var glowFrame = texture.Frame(frameCount, 6, (int)MathF.Floor(progress * frameCount), variant % 3 + 3);

        SpriteEffects spriteEffects = 0;
        if (variant > 3)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }

        var drawScale = Scale * MathF.Cbrt(progress);

        var lightColor = Color.White;

        if (ApplyLighting)
        {
            lightColor = Lighting.GetColor(Position.ToTileCoordinates());
        }

        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, ColorTint.MultiplyRGBA(lightColor), Rotation, frame.Size() / 2, drawScale, spriteEffects, 0);
        spritebatch.Draw(texture, Position + settings.AnchorPosition, glowFrame, ColorInsideTint.MultiplyRGBA(lightColor), Rotation, frame.Size() / 2, drawScale, spriteEffects, 0);
    }
}