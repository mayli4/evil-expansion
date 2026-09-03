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

internal readonly struct RenderPipeline : IDisposable {
    private readonly RenderCommandQueue _queue;
    private readonly int _depth;

    public RenderPipeline(RenderCommandQueue queue, float scale, Matrix? matrix) : this(queue, 0, scale, matrix) { }

    private RenderPipeline(RenderCommandQueue queue, int depth, float scale, Matrix? matrix) {
        _queue = queue;
        queue.AddBegin(scale, matrix);

        _depth = depth;
    }

    public readonly RenderPipeline DrawTexture(DrawTextureOptions options) {
        var source = options.Source ?? new Vector4(0f, 0f, options.Texture.Width, options.Texture.Height);
        _queue.AddDrawTexture(
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
        TrailRenderer.WidthFunction widthFn,
        TrailRenderer.ColorFunc colorFn,
        Effect? effect = null,
        int spriteRotation = 0
    ) {
        _queue.AddDrawTrail(
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
        _queue.AddApplyEffect(effect);
        return this;
    }

    public readonly RenderPipeline ApplyEffect(Effect effect, params ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        _queue.AddSetEffectParams(effect, parameters);
        _queue.AddApplyEffect(effect);
        return this;
    }

    public readonly RenderPipeline Clear(Color color) {
        _queue.AddClear(color);
        return this;
    }

    public readonly RenderPipeline SetTexture(Texture2D texture) {
        _queue.AddSetTexture(0, texture);
        return this;
    }

    public readonly RenderPipeline SetTexture(int index, Texture2D texture) {
        _queue.AddSetTexture(index, texture);
        return this;
    }

    public readonly RenderPipeline SetTexture(Texture2D texture, SamplerState samplerState) {
        _queue.AddSetTexture(0, texture);
        _queue.AddSetSamplerState(0, samplerState);
        return this;
    }

    public readonly RenderPipeline SetTexture(int index, Texture2D texture, SamplerState samplerState) {
        _queue.AddSetTexture(index, texture);
        _queue.AddSetSamplerState(index, samplerState);
        return this;
    }

    public readonly RenderPipeline SetSamplerState(SamplerState samplerState) {
        _queue.AddSetSamplerState(0, samplerState);
        return this;
    }

    public readonly RenderPipeline SetSamplerState(int index, SamplerState samplerState) {
        _queue.AddSetSamplerState(index, samplerState);
        return this;
    }

    public readonly RenderPipeline SetEffectParams(Effect effect, params ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        _queue.AddSetEffectParams(effect, parameters);
        return this;
    }

    public readonly RenderPipeline SetBlendState(BlendState blendState) {
        _queue.AddSetBlendState(blendState);
        return this;
    }

    public readonly RenderPipeline Begin(float scale = 1f, Matrix? matrix = null) {
        return new(_queue, _depth + 1, scale, matrix);
    }

    public readonly void End() {
        _queue.AddEnd();

        if(_depth == 0) {
            RenderCommandRunner.Instance.Run(_queue);
            _queue.Clear();
        }
    }

    public void Dispose() => End();
}

