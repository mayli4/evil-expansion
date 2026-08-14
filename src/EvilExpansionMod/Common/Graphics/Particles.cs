using Daybreak.Common.Features.Hooks;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Renderers;

namespace EvilExpansionMod.Common.Graphics;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PoolCapacityAttribute(int size) : Attribute {
    public int Capacity { get; } = size;
}

public abstract class BaseParticle<T> : IPooledParticle where T : BaseParticle<T>, new() {
    public const int DEFAULT_POOL_CAPACITY = 200;

    public static ParticlePool<T> Pool { get; } = new(typeof(T).GetCustomAttribute<PoolCapacityAttribute>(inherit: false)?.Capacity ?? DEFAULT_POOL_CAPACITY, GetNewParticle);

    public bool IsRestingInPool { get; private set; }

    public bool ShouldBeRemovedFromRenderer { get; protected set; }

    public virtual void FetchFromPool() {
        IsRestingInPool = false;
        ShouldBeRemovedFromRenderer = false;
    }

    public virtual void RestInPool() {
        IsRestingInPool = true;
    }

    public virtual void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) { }

    public virtual void Update(ref ParticleRendererSettings settings) { }

    protected static T GetNewParticle() {
        return new T();
    }
}

public static class ParticleEngine {
    /// <summary>
    ///     Renders behind dust.
    /// </summary>
    public static readonly ParticleRenderer PARTICLES = new();

    /// <summary>
    ///     Renders behind front gore.
    /// </summary>
    public static readonly ParticleRenderer GORE_LAYER = new();
    
    
    public static readonly ParticleRenderer BEHIND_PROJECTILES = new();

    [OnLoad]
    public static void Load() {
        On_Main.DrawProjectiles += DrawBackgroundParticles;
        On_Main.DrawDust += DrawParticles;
        On_Main.DrawGore += DrawGoreParticles;
        On_Main.UpdateParticleSystems += UpdateParticles;
    }

    private static void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self) {
        orig(self);

        BEHIND_PROJECTILES.Update();
        PARTICLES.Update();
        GORE_LAYER.Update();
    }
    
    private static void DrawBackgroundParticles(On_Main.orig_DrawProjectiles orig, Main self) {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        
        BEHIND_PROJECTILES.Settings.AnchorPosition = -Main.screenPosition;
        BEHIND_PROJECTILES.Draw(Main.spriteBatch);
        
        Main.spriteBatch.End();
        orig(self);
    }

    private static void DrawParticles(On_Main.orig_DrawDust orig, Main self) {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        PARTICLES.Settings.AnchorPosition = -Main.screenPosition;
        PARTICLES.Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        orig(self);
    }

    private static void DrawGoreParticles(On_Main.orig_DrawGore orig, Main self) {
        //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        GORE_LAYER.Settings.AnchorPosition = -Main.screenPosition;
        GORE_LAYER.Draw(Main.spriteBatch);
        //Main.spriteBatch.End();

        orig(self);
    }
}