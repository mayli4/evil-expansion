using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EvilExpansionMod.Common.Graphics;


internal class RenderCommandQueue(bool immediate = false) {
    public bool Immediate { get; } = immediate;

    public List<RenderCommandTag> Tags = [];
    public List<int> Indices = [];

    public List<BeginData> BeginData = [];
    public List<DrawTextureData> DrawTextureData = [];
    public List<DrawTrailData> DrawTrailData = [];
    public List<SetEffectParamsData> SetEffectParamsData = [];
    public List<SetTextureData> SetTextureData = [];
    public List<SetSamplerState> SetSamplerStateData = [];

    public List<Color> Colors = [];
    public List<Vector2> Positions = [];
    public List<Matrix> Matrices = [];
    public List<Effect> Effects = [];
    public List<Texture2D> Textures = [];
    public List<SamplerState> SamplerStates = [];
    public List<BlendState> BlendStates = [];
    public List<(string, EffectParameterValue)> EffectParams = [];

    public void Clear() {
        Tags.Clear();
        Indices.Clear();

        BeginData.Clear();
        DrawTextureData.Clear();
        DrawTrailData.Clear();
        SetEffectParamsData.Clear();
        SetTextureData.Clear();
        SetSamplerStateData.Clear();

        Colors.Clear();
        Positions.Clear();
        Matrices.Clear();
        Effects.Clear();
        Textures.Clear();
        SamplerStates.Clear();
        BlendStates.Clear();
        EffectParams.Clear();
    }

    public void AddBegin(float scale, Matrix? matrix) {
        Tags.Add(RenderCommandTag.Begin);

        var matrixIndex = -1;
        if(matrix != null) {
            matrixIndex = Matrices.Count;
            Matrices.Add(matrix.Value);
        }

        var index = BeginData.Count;
        BeginData.Add(new(scale, matrixIndex));
        Indices.Add(index);
    }

    public void AddEnd() {
        Tags.Add(RenderCommandTag.End);
        Indices.Add(-1);
    }

    public void AddDrawTexture(
        Texture2D texture,
        Vector2 position,
        Color color,
        Vector4 source,
        float rotation,
        Vector2 size,
        Vector2 origin,
        SpriteEffects spriteEffects,
        Effect? effect
    ) {
        var sin = MathF.Sin(rotation);
        var cos = MathF.Cos(rotation);

        var rotatedOrigin = new Vector2(
            origin.X * cos - origin.Y * sin,
            origin.X * sin + origin.Y * cos);

        var bottomLeft = position - rotatedOrigin;

        var right = new Vector2(cos, sin);
        var bottomRight = bottomLeft + right * size.X;

        var upScaled = new Vector2(-right.Y, right.X) * size.Y;
        var topLeft = bottomLeft + upScaled;
        var topRight = bottomRight + upScaled;

        var positionDataIndex = Positions.Count;
        Positions.AddRange([bottomRight, topRight, bottomLeft, topLeft]);

        var sourceNormalized = new Vector4(
            source.X / texture.Width,
            source.Y / texture.Height,
            source.Z / texture.Width,
            source.W / texture.Height
        );

        ReadOnlySpan<float> offX = [0f, 1f, 0f, 1f];
        ReadOnlySpan<float> offY = [0f, 0f, 1f, 1f];

        var effects = (byte)spriteEffects;
        sourceNormalized.X += sourceNormalized.Z * offX[effects];
        sourceNormalized.Y += sourceNormalized.W * offY[effects];
        sourceNormalized.Z -= 2f * sourceNormalized.Z * offX[effects];
        sourceNormalized.W -= 2f * sourceNormalized.W * offY[effects];

        Tags.Add(RenderCommandTag.DrawTexture);

        var index = DrawTextureData.Count;
        DrawTextureData.Add(new()
        {
            Texture = texture,
            PositionDataIndex = positionDataIndex,
            Color = color,
            Source = sourceNormalized,
            Effect = effect
        });

        Indices.Add(index);
    }

    public void AddDrawTrail(
        ReadOnlySpan<Vector2> positions,
        TrailRenderer.WidthFunction widthFn,
        TrailRenderer.ColorFunc colorFn,
        Effect? effect,
        int spriteRotation
    ) {
        if(positions.Length < 2) return;

        var positionDataIndex = Positions.Count;
        Positions.AddRange(positions);

        Tags.Add(RenderCommandTag.DrawTrail);

        var index = DrawTrailData.Count;
        DrawTrailData.Add(new()
        {
            PositionsIndex = positionDataIndex,
            PositionCount = positions.Length,
            WidthFn = widthFn,
            ColorFn = colorFn,
            Effect = effect,
            SpriteRotation = spriteRotation,
        });

        Indices.Add(index);
    }

    public void AddApplyEffect(Effect effect) {
        Tags.Add(RenderCommandTag.ApplyEffect);

        var index = Effects.Count;
        Effects.Add(effect);

        Indices.Add(index);
    }

    public void AddClear(Color color) {
        Tags.Add(RenderCommandTag.Clear);

        var index = Colors.Count;
        Colors.Add(color);

        Indices.Add(index);
    }

    public void AddSetTexture(int index, Texture2D texture) {
        Tags.Add(RenderCommandTag.SetTexture);

        var textureDataIndex = Textures.Count;
        Textures.Add(texture);

        var dataIndex = SetTextureData.Count;
        SetTextureData.Add(new()
        {
            Index = index,
            TextureIndex = textureDataIndex,
        });

        Indices.Add(dataIndex);
    }

    public void AddSetSamplerState(int index, SamplerState samplerState) {
        Tags.Add(RenderCommandTag.SetSamplerState);

        var samplerStateDataIndex = SamplerStates.Count;
        SamplerStates.Add(samplerState);

        var dataIndex = SetSamplerStateData.Count;
        SetSamplerStateData.Add(new()
        {
            Index = index,
            SamplerStateIndex = samplerStateDataIndex,
        });

        Indices.Add(dataIndex);
    }

    public void AddSetEffectParams(Effect effect, ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        Tags.Add(RenderCommandTag.SetEffectParams);

        var effectParamsIndex = EffectParams.Count;
        var effectParamsCount = parameters.Length;

        EffectParams.AddRange(parameters);

        var dataIndex = SetEffectParamsData.Count;
        SetEffectParamsData.Add(new(effect, effectParamsIndex, effectParamsCount));

        Indices.Add(dataIndex);
    }

    public void AddSetBlendState(BlendState blendState) {
        Tags.Add(RenderCommandTag.SetBlendState);

        var index = BlendStates.Count;
        BlendStates.Add(blendState);

        Indices.Add(index);
    }
}

internal enum RenderCommandTag : byte {
    Begin,
    End,

    DrawTexture,
    DrawTrail,
    ApplyEffect,
    Clear,

    SetTexture,
    SetSamplerState,
    SetBlendState,
    SetEffectParams,
}

internal record struct BeginData(
    float Scale,
    int MatrixIndex);

internal record struct DrawTextureData(
    Texture2D Texture,
    Color Color,
    int PositionDataIndex,
    Vector4 Source,
    Effect? Effect);

internal record struct DrawTrailData(
    int PositionsIndex,
    int PositionCount,
    TrailRenderer.WidthFunction WidthFn,
    TrailRenderer.ColorFunc ColorFn,
    Effect? Effect,
    int SpriteRotation);

internal record struct SetTextureData(
    int Index,
    int TextureIndex);

internal record struct SetSamplerState(
    int Index,
    int SamplerStateIndex);

internal record struct SetEffectParamsData(
    Effect Effect,
    int EffectParamsIndex,
    int EffectParamCount);

[StructLayout(LayoutKind.Explicit)]
internal struct EffectParameterValue {
    [FieldOffset(0)]
    public EffectParameterValueType Type;

    // NOTE: References cannot be overlapped unfortunately..
    [FieldOffset(8)]
    public Texture2D Texture2D;

    [FieldOffset(16)]
    public float Float;

    [FieldOffset(16)]
    public int Int;

    [FieldOffset(16)]
    public Vector2 Vector2;

    [FieldOffset(16)]
    public Vector3 Vector3;

    [FieldOffset(16)]
    public Vector4 Vector4;

    [FieldOffset(16)]
    public Matrix Matrix;

    public static implicit operator EffectParameterValue(float value) => new()
    {
        Type = EffectParameterValueType.Float,
        Float = value,
    };

    public static implicit operator EffectParameterValue(int value) => new()
    {
        Type = EffectParameterValueType.Int,
        Int = value,
    };

    public static implicit operator EffectParameterValue(Vector2 value) => new()
    {
        Type = EffectParameterValueType.Vector2,
        Vector2 = value,
    };

    public static implicit operator EffectParameterValue(Vector3 value) => new()
    {
        Type = EffectParameterValueType.Vector3,
        Vector3 = value,
    };

    public static implicit operator EffectParameterValue(Vector4 value) => new()
    {
        Type = EffectParameterValueType.Vector4,
        Vector4 = value,
    };

    public static implicit operator EffectParameterValue(Texture2D value) => new()
    {
        Type = EffectParameterValueType.Texture2D,
        Texture2D = value,
    };

    public static implicit operator EffectParameterValue(Matrix value) => new()
    {
        Type = EffectParameterValueType.Matrix,
        Matrix = value,
    };

    public static implicit operator EffectParameterValue(Color value) => new()
    {
        Type = EffectParameterValueType.Vector4,
        Vector4 = value.ToVector4(),
    };
}

internal enum EffectParameterValueType {
    Float,
    Int,
    Vector2,
    Vector3,
    Vector4,
    Texture2D,
    Matrix,
}
