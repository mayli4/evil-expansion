using Daybreak.Common.Features.Hooks;
using EvilExpansionMod.Common.Graphics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;

namespace EvilExpansionMod.Content.Particles;

[PoolCapacity(100)]
internal sealed class BloodParticle : BaseParticle<BloodParticle> {
    public Vector2 Position;
    public Vector2 OldPosition;
    public Vector2 Velocity;
    public float Scale;
    public Color ColorTint;

    public float LifeTime;
    public Vector2 Gravity;
    public float Friction;

    public int FrameIndex;

    public static BloodParticle NewParticle(Vector2 position, Vector2 velocity, float scale, Color tintColor) {
        var particle = Pool.RequestParticle();
        particle.Position = position;
        particle.OldPosition = position;
        particle.Velocity = velocity;
        particle.Scale = scale;
        particle.ColorTint = tintColor;

        return particle;
    }

    public override void FetchFromPool() {
        base.FetchFromPool();
        LifeTime = 0;
        Scale = 1f;
        Gravity = Vector2.UnitY * 0.35f;
        Friction = 0.98f;
        FrameIndex = Main.rand.Next(2);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        LifeTime++;
        OldPosition = Position;

        Velocity += Gravity;
        Velocity.X *= Friction;

        Vector2 collisionSize = new Vector2(4, 4);
        Vector2 originOffset = collisionSize * 0.5f;

        Vector2 oldVelocity = Velocity;
        Velocity = Collision.TileCollision(Position - originOffset, Velocity, (int)collisionSize.X, (int)collisionSize.Y, true, true);

        if (Velocity.X != oldVelocity.X || Velocity.Y != oldVelocity.Y) {
            ShouldBeRemovedFromRenderer = true;

            for (int i = 0; i < 3; i++) {
                var dust = Dust.NewDustPerfect(
                    Position, 
                    DustID.Blood, 
                    -oldVelocity * Main.rand.NextFloat(0.1f, 0.3f) + Main.rand.NextVector2Circular(1f, 1f), 
                    0, 
                    ColorTint, 
                    Main.rand.NextFloat(0.6f, 1f)
                );
                dust.noGravity = false;
            }
            return;
        }

        if (Scale < 0.08f || LifeTime > 240) {
            ShouldBeRemovedFromRenderer = true;
            return;
        }

        Position += Velocity;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        float fadeOut = Utils.Remap(Scale, 0.08f, 0.25f, 0f, 1f);
        float spawnScale = Utils.Remap(LifeTime, 0f, 3f, 0.2f, 1f);

        Vector2 currentDrawPos = Position + settings.AnchorPosition;
        Vector2 oldDrawPos = OldPosition + settings.AnchorPosition;
        Vector2 trailVector = currentDrawPos - oldDrawPos;
        float stepLength = trailVector.Length();

        if (stepLength > 0.1f) {
            float rot = trailVector.ToRotation() + MathHelper.Pi;
        
            var trailScale = new Vector2(
                stepLength * 3f, 
                2
            );

            spritebatch.Draw(
                TextureAssets.MagicPixel.Value,
                currentDrawPos,
                new Rectangle(0, 0, 1, 1),
                new Color(106, 32, 20) * fadeOut,
                rot,
                new Vector2(0f, 0.5f),
                trailScale,
                SpriteEffects.None,
                0f
            );
        }

        var texture = Assets.Images.Particles.BloodParticle.Asset.Value;
        int frameWidth = texture.Width / 3;
        int frameHeight = texture.Height;
        var sourceRectangle = new Rectangle(FrameIndex * frameWidth, 0, frameWidth, frameHeight);
        var origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
        float rotation = Velocity.ToRotation() + MathHelper.PiOver2;

        spritebatch.Draw(
            texture,
            currentDrawPos,
            sourceRectangle,
            Lighting.GetColor(Position.ToTileCoordinates()),
            rotation,
            origin,
            1,
            SpriteEffects.None,
            0f
        );
    }
}