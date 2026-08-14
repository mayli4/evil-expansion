using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EvilExpansionMod.Common.Graphics;


internal readonly struct DrawTextureOptions() {
    public required Texture2D Texture { get; init; }
    public required Vector2 Position { get; init; }
    public Color Color { get; init; } = Color.White;
    public Vector4? Source { get; init; } = null;
    public float Rotation { get; init; } = 0f;
    public Vector2 Origin { get; init; } = Vector2.Zero;
    public Vector2 Scale { get; init; } = Vector2.One;
    public SpriteEffects SpriteEffects { get; init; } = SpriteEffects.None;
    public Vector2? Size { get; init; } = null;
    public Effect? Effect { get; init; } = null;
    public BlendState BlendState { get; init; } = BlendState.AlphaBlend;
}

internal readonly struct RenderPipeline(Renderer graphics, int depth) : IDisposable {
    public readonly RenderPipeline DrawTexture(DrawTextureOptions options) {
        var source = options.Source ?? new Vector4(0f, 0f, options.Texture.Width, options.Texture.Height);
        graphics.AddDrawTexture(
            options.Texture,
            options.Position,
            options.Color,
            source,
            options.Rotation,
            options.Size != null ?
                options.Size.Value * options.Scale :
                new Vector2(source.Z, source.W) * options.Scale,
            options.Origin * options.Scale,
            options.SpriteEffects,
            options.Effect
        );

        return this;
    }

    public readonly RenderPipeline DrawTrail(
        ReadOnlySpan<Vector2> positions,
        Func<float, float> widthFn,
        Func<float, Color> colorFn,
        Effect? effect = null,
        int spriteRotation = 0
    ) {
        graphics.AddDrawTrail(
            positions,
            widthFn,
            colorFn,
            effect,
            spriteRotation);

        return this;
    }

    public readonly RenderPipeline DrawTrail(
        ReadOnlySpan<Vector2> positions,
        float width,
        Color color,
        Effect? effect = null,
        int spriteRotation = 0
    ) {
        return DrawTrail(
            positions,
            _ => width,
            _ => color,
            effect,
            spriteRotation);
    }

    public readonly RenderPipeline ApplyEffect(Effect effect) {
        graphics.AddApplyEffect(effect);
        return this;
    }

    public readonly RenderPipeline ApplyEffect(Effect effect, params ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        graphics.AddSetEffectParams(effect, parameters);
        graphics.AddApplyEffect(effect);
        return this;
    }

    public readonly RenderPipeline Clear(Color color) {
        graphics.AddClear(color);
        return this;
    }

    public readonly RenderPipeline SetTexture(Texture2D texture) {
        graphics.AddSetTexture(0, texture);
        return this;
    }

    public readonly RenderPipeline SetTexture(int index, Texture2D texture) {
        graphics.AddSetTexture(index, texture);
        return this;
    }

    public readonly RenderPipeline SetSamplerState(SamplerState samplerState) {
        graphics.AddSetSamplerState(0, samplerState);
        return this;
    }

    public readonly RenderPipeline SetSamplerState(int index, SamplerState samplerState) {
        graphics.AddSetSamplerState(index, samplerState);
        return this;
    }

    public readonly RenderPipeline SetEffectParams(Effect effect, params ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        graphics.AddSetEffectParams(effect, parameters);
        return this;
    }

    public readonly RenderPipeline SetBlendState(BlendState blendState) {
        graphics.AddSetBlendState(blendState);
        return this;
    }

    public readonly RenderPipeline Begin(float scale = 1f, Matrix? matrix = null) {
        return graphics.BeginPipeline(scale, matrix, depth + 1);
    }

    public readonly void End() {
        graphics.EndPipeline();
        if(depth == 0) graphics.Flush();
    }

    public void Dispose() => End();

}

