using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Core;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Particles;

internal sealed class SmokeParticle : BaseParticle<SmokeParticle> {
    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
    public float Rotation;
    public float RotationSpeed;
    public float Alpha;
    public int Lifetime;
    public int MaxLifetime;
    public Color BaseColor;

    public override void FetchFromPool() {
        base.FetchFromPool();
        Position = Vector2.Zero;
        Velocity = Vector2.Zero;
        Scale = 1f;
        Rotation = 0f;
        RotationSpeed = 0f;
        Alpha = 1f;
        Lifetime = 0;
        MaxLifetime = 60;
        BaseColor = Color.Black;
    }

    public void Spawn(Vector2 position, Vector2 velocity, Color color, float scale = 1f, int maxLifetime = 60) {
        Position = position;
        Velocity = velocity;
        BaseColor = color;
        Scale = scale;
        MaxLifetime = maxLifetime;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        Alpha = 1f;
        Lifetime = 0;

        SmokeParticleRendering.Particles.Add(this);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        Lifetime++;
        if (Lifetime >= MaxLifetime) {
            ShouldBeRemovedFromRenderer = true;
            return;
        }

        Position += Velocity;
        Velocity.X *= 0.95f;
        Velocity.Y -= 0.03f;
        
        Rotation += RotationSpeed;
        Scale += 0.015f;
        float progress = (float)Lifetime / MaxLifetime;
        Alpha = MathF.Sin(progress * MathHelper.Pi) * 0.8f;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        Texture2D texture = Assets.Images.Particles.Smoke.Asset.Value; 
        
        var drawPosition = Position + settings.AnchorPosition;
        var origin = texture.Size() * 0.5f;
        var drawColor = BaseColor * Alpha;

        spriteBatch.Draw(
            texture,
            drawPosition,
            null,
            drawColor,
            Rotation,
            origin,
            Scale,
            SpriteEffects.None,
            0f
        );
    }
}

[UsedImplicitly]
file static class SmokeParticleRendering {
    [Autoload(Side = ModSide.Client)]
    private sealed class Data : IStatic<Data> {
        public required RenderTargetLease CloudLease { get; init; }

        public static Data LoadData(Mod mod) {
            return Main.RunOnMainThread(
                () => new Data
                {
                    CloudLease = ScreenspaceTargetPool.Shared.Rent(
                        Main.instance.GraphicsDevice,
                        (w, h) => (w / 2, h / 2)
                    ),
                }
            ).GetAwaiter().GetResult();
        }

        public static void UnloadData(Data data) {
            Main.RunOnMainThread(
                () =>
                {
                    data.CloudLease.Dispose();
                }
            );
        }
    }
    
    public static readonly ParticleRenderer Particles = new();

    [OnLoad, UsedImplicitly]
    private static void ApplyHooks() {
        On_Main.DrawDust += DrawCloudParticles;
    }
    
    [ModSystemHooks.PostUpdateEverything, UsedImplicitly]
    private static void UpdateParticles() {
        Particles.Update();
    }
    
    private static void DrawCloudParticles(On_Main.orig_DrawDust orig, Main self) {
        orig(self);

        var cloudLease = IStatic<Data>.Instance.CloudLease;
        var sb = Main.spriteBatch;

        sb.Begin();
        sb.End(out var ss);

        using (cloudLease.Scope(clearColor: Color.Transparent)) {
            sb.Begin(ss with { SamplerState = SamplerState.PointClamp, TransformMatrix = Matrix.CreateScale(0.5f), BlendState = BlendState.AlphaBlend });

            Particles.Settings.AnchorPosition = -Main.screenPosition;
            Particles.Draw(sb);

            sb.End();
        }

        using (sb.Scope()) {
            var shader = Assets.Shaders.Pixel.Smoke.Asset.Value;
            var displacementMapTex = Assets.Images.Sample.Flame1.Asset.Value;

            var targetSize = new Vector2(cloudLease.Target.Width, cloudLease.Target.Height);

            shader.Parameters["textureResolution"]?.SetValue(targetSize);
            shader.Parameters["worldPos"]?.SetValue(Main.screenPosition);
            shader.Parameters["noiseMap"]?.SetValue(displacementMapTex);
            shader.Parameters["noiseStrength"]?.SetValue(0.02f);
            shader.Parameters["baseColor"]?.SetValue(new Vector4(0.2f, 0.2f, 0.2f, 1f));
            shader.Parameters["shadowColor"]?.SetValue(new Vector4(0.4f, 0.4f, 0.4f, 1f));
            shader.Parameters["outlineColor"]?.SetValue(new Vector4(0.4f, 0.4f, 0.4f, 1f));
            
            sb.Begin(
                ss with { 
                    SamplerState = SamplerState.PointClamp, 
                    TransformMatrix = Main.Transform, 
                    RasterizerState = Main.Rasterizer,
                    CustomEffect = shader,
                }
            );

            sb.Draw(
                cloudLease.Target,
                Vector2.Zero,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                0f
            );

            sb.End(out var ss2);
            sb.Begin(ss2);
        }
    }
}