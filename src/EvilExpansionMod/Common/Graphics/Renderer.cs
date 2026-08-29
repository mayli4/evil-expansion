using Daybreak.Common.Rendering;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal class Renderer : ModSystem {
    static Renderer s_Instance = null!;

    static GraphicsDevice Device => Main.graphics.GraphicsDevice;

    RenderTarget2D _swapTarget = null!;
    RenderTarget2D? _drawTarget;

    readonly List<RenderTarget2D> _targetPool = [];
    int _targetPoolIndex;

    RenderCommands _commands = new();

    static Effect QuadEffect => Assets.Shaders.Core.Quad.Asset.Value;
    DynamicVertexBuffer _quadVertexBuffer = null!;

    const int TrailPositionCapacity = 256;
    const int TrailVertexCount = TrailPositionCapacity * 2;
    const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    static Effect TrailEffect => Assets.Shaders.Core.Trail.Asset.Value;
    DynamicVertexBuffer _trailVertexBuffer = null!;
    readonly VertexPositionColorTexture[] _trailVertices = new VertexPositionColorTexture[TrailVertexCount];

    DynamicIndexBuffer _trailIndexBuffer = null!;
    readonly ushort[] _trailIndices = new ushort[TrailIndexCount];

    Matrix _matrix = Matrix.Identity;
    float _scale = 1f;

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            _swapTarget = CreateRenderTarget();
            _drawTarget = CreateRenderTarget();

            for(var i = 0; i < 4; i++) _targetPool.Add(CreateRenderTarget());

            _quadVertexBuffer = new DynamicVertexBuffer(Device, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            _trailVertexBuffer = new(
                Device,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly);

            _trailIndexBuffer = new(
                Device,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly);
        });

        s_Instance = this;
    }

    public override void PostDrawInterface(SpriteBatch spriteBatch) {
        _commands.Clear(); // NOTE: In case there is some left for whatever reason.
    }

    public static RenderPipeline Begin(Matrix? matrix = null) {
        return Begin(2f, matrix);
    }

    public static RenderPipeline BeginPixelated(Matrix? matrix = null) {
        return Begin(0.5f, matrix);
    }

    public static RenderPipeline Begin(float scale, Matrix? matrix = null)
        => s_Instance.BeginPipeline(scale, matrix ?? Graphics.ScreenTransformMatrix, 0);

    public RenderPipeline BeginPipeline(float scale, Matrix? matrix, int depth) {
        _commands.Tags.Add(CommandTag.Begin);

        var matrixIndex = -1;
        if(matrix != null) {
            matrixIndex = _commands.Matrices.Count;
            _commands.Matrices.Add(matrix.Value);
        }

        var index = _commands.BeginData.Count;
        _commands.BeginData.Add(new(scale, matrixIndex));
        _commands.Indices.Add(index);

        return new(this, depth);
    }

    public void EndPipeline() {
        _commands.Tags.Add(CommandTag.End);
        _commands.Indices.Add(-1);
    }

    public void Flush() {
        Device.BlendState = BlendState.AlphaBlend;
        Device.SamplerStates[0] = SamplerState.PointWrap;
        Device.SamplerStates[1] = SamplerState.PointWrap;
        Device.SamplerStates[2] = SamplerState.PointWrap;
        Device.SamplerStates[3] = SamplerState.PointWrap;
        Device.RasterizerState = RasterizerState.CullClockwise;

        SpriteBatchSnapshot? spriteBatchSnapshot = null;
        if(Main.spriteBatch.beginCalled) {
            Main.spriteBatch.End(out var snapshot);
            spriteBatchSnapshot = snapshot;
        }

        RenderTargetUsage? cachedTargetUsage = null;
        var targets = Device.GetRenderTargets();
        if(targets.Length > 0) {
            // NOTE: This is gonna get swapped in the first `Begin` call, and then replaced in the last `End` call.
            _drawTarget = (RenderTarget2D)targets[0].RenderTarget;

            cachedTargetUsage = _drawTarget.RenderTargetUsage;
            _drawTarget.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
        else {
            _drawTarget = null;
        }

        var beginCount = 0;
        var commandsCount = _commands.Tags.Count;
        for(var i = 0; i < commandsCount; i++) {
            var dataIndex = _commands.Indices[i];
            switch(_commands.Tags[i]) {
                case CommandTag.Begin:
                    beginCount++;
                    ExecuteBegin(dataIndex);
                    break;
                case CommandTag.End:
                    beginCount--;
                    ExecuteEnd();
                    break;
                case CommandTag.DrawTexture:
                    ExecuteDrawTexture(dataIndex);
                    break;
                case CommandTag.DrawTrail:
                    ExecuteDrawTrail(dataIndex);
                    break;
                case CommandTag.ApplyEffect:
                    ExecuteApplyEffect(dataIndex);
                    break;
                case CommandTag.Clear:
                    ExecuteClear(dataIndex);
                    break;
                case CommandTag.SetTexture:
                    ExecuteSetTexture(dataIndex);
                    break;
                case CommandTag.SetSamplerState:
                    ExecuteSetSamplerState(dataIndex);
                    break;
                case CommandTag.SetBlendState:
                    ExecuteSetBlendState(dataIndex);
                    break;
                case CommandTag.SetEffectParams:
                    ExecuteSetEffectParams(dataIndex);
                    break;
            }
        }

        if(_drawTarget != null) {
            _drawTarget.RenderTargetUsage = cachedTargetUsage!.Value;
        }

        if(spriteBatchSnapshot is SpriteBatchSnapshot snap) {
            Main.spriteBatch.Begin(snap);
        }

        _commands.Clear();

        if(beginCount != 0) {
            throw new InvalidOperationException("Begin and end command count mismatch");
        }
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

        var positionDataIndex = _commands.Positions.Count;
        _commands.Positions.AddRange([bottomRight, topRight, bottomLeft, topLeft]);

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

        _commands.Tags.Add(CommandTag.DrawTexture);

        var index = _commands.DrawTextureData.Count;
        _commands.DrawTextureData.Add(new()
        {
            Texture = texture,
            PositionDataIndex = positionDataIndex,
            Color = color,
            Source = sourceNormalized,
            Effect = effect
        });

        _commands.Indices.Add(index);
    }

    public void AddDrawTrail(
        ReadOnlySpan<Vector2> positions,
        Func<float, float> widthFn,
        Func<float, Color> colorFn,
        Effect? effect,
        int spriteRotation
    ) {
        if(positions.Length < 2) return;

        var positionDataIndex = _commands.Positions.Count;
        _commands.Positions.AddRange(positions);

        _commands.Tags.Add(CommandTag.DrawTrail);

        var index = _commands.DrawTrailData.Count;
        _commands.DrawTrailData.Add(new()
        {
            PositionsIndex = positionDataIndex,
            PositionCount = positions.Length,
            WidthFn = widthFn,
            ColorFn = colorFn,
            Effect = effect,
            SpriteRotation = spriteRotation,
        });

        _commands.Indices.Add(index);
    }

    public void AddApplyEffect(Effect effect) {
        _commands.Tags.Add(CommandTag.ApplyEffect);

        var index = _commands.Effects.Count;
        _commands.Effects.Add(effect);

        _commands.Indices.Add(index);
    }

    public void AddClear(Color color) {
        _commands.Tags.Add(CommandTag.Clear);

        var index = _commands.Colors.Count;
        _commands.Colors.Add(color);

        _commands.Indices.Add(index);
    }

    public void AddSetTexture(int index, Texture2D texture) {
        _commands.Tags.Add(CommandTag.SetTexture);

        var textureDataIndex = _commands.Textures.Count;
        _commands.Textures.Add(texture);

        var dataIndex = _commands.SetTextureData.Count;
        _commands.SetTextureData.Add(new()
        {
            Index = index,
            TextureIndex = textureDataIndex,
        });

        _commands.Indices.Add(dataIndex);
    }

    public void AddSetSamplerState(int index, SamplerState samplerState) {
        _commands.Tags.Add(CommandTag.SetSamplerState);

        var samplerStateDataIndex = _commands.SamplerStates.Count;
        _commands.SamplerStates.Add(samplerState);

        var dataIndex = _commands.SetSamplerStateData.Count;
        _commands.SetSamplerStateData.Add(new()
        {
            Index = index,
            SamplerStateIndex = samplerStateDataIndex,
        });

        _commands.Indices.Add(dataIndex);
    }

    public void AddSetEffectParams(Effect effect, ReadOnlySpan<(string, EffectParameterValue)> parameters) {
        _commands.Tags.Add(CommandTag.SetEffectParams);

        var effectParamsIndex = _commands.EffectParams.Count;
        var effectParamsCount = parameters.Length;

        _commands.EffectParams.AddRange(parameters);

        var dataIndex = _commands.SetEffectParamsData.Count;
        _commands.SetEffectParamsData.Add(new(effect, effectParamsIndex, effectParamsCount));

        _commands.Indices.Add(dataIndex);
    }

    public void AddSetBlendState(BlendState blendState) {
        _commands.Tags.Add(CommandTag.SetBlendState);

        var index = _commands.BlendStates.Count;
        _commands.BlendStates.Add(blendState);

        _commands.Indices.Add(index);
    }

    void ExecuteBegin(int dataIndex) {
        if(_targetPoolIndex == _targetPool.Count) {
            var rt = CreateRenderTarget();
            _targetPool.Add(rt);
        }

        var oldViewportWidth = Device.Viewport.Width;
        var oldViewportHeight = Device.Viewport.Height;

        (_targetPool[_targetPoolIndex], _drawTarget) = (_drawTarget!, _targetPool[_targetPoolIndex]);
        _targetPoolIndex++;

        Device.SetRenderTarget(_drawTarget);
        Device.Clear(Color.Transparent);

        var beginData = _commands.BeginData[dataIndex];

        _scale = beginData.Scale;
        if(beginData.MatrixIndex > -1) _matrix = _commands.Matrices[beginData.MatrixIndex];

        Device.Viewport = new(
            0,
            0,
            (int)(oldViewportWidth * _scale / Main.GameViewMatrix.Zoom.X),
            (int)(oldViewportHeight * _scale / Main.GameViewMatrix.Zoom.X));
    }

    void ExecuteEnd() {
        _targetPoolIndex--;
        (_targetPool[_targetPoolIndex], _drawTarget) = (_drawTarget!, _targetPool[_targetPoolIndex]);

        Device.SetRenderTarget(_drawTarget);

        var viewportWidth = Device.Viewport.Width;
        var viewportHeight = Device.Viewport.Height;

        Device.BlendState = BlendState.AlphaBlend;
        Device.SamplerStates[0] = SamplerState.PointClamp;

        var oldTarget = _targetPool[_targetPoolIndex];
        var viewportTargetRatio = new Vector2(
            (float)viewportWidth / oldTarget.Width,
            (float)viewportHeight / oldTarget.Height);

        var source = new Vector4(
            0,
            0,
            _scale * viewportTargetRatio.X / Main.GameViewMatrix.Zoom.X,
            _scale * viewportTargetRatio.Y / Main.GameViewMatrix.Zoom.Y);

        DrawQuad(
            oldTarget,
            [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)],
            source,
            Color.White,
            Matrix.Identity,
            null);
    }

    void ExecuteDrawTexture(int dataIndex) {
        var data = _commands.DrawTextureData[dataIndex];
        var positions = CollectionsMarshal.AsSpan(_commands.Positions)[data.PositionDataIndex..(data.PositionDataIndex + 4)];

        DrawQuad(data.Texture, positions, data.Source, data.Color, _matrix, data.Effect);
    }

    void ExecuteDrawTrail(int dataIndex) {
        var data = _commands.DrawTrailData[dataIndex];
        var positions = CollectionsMarshal.AsSpan(_commands.Positions)[data.PositionsIndex..(data.PositionsIndex + data.PositionCount)];

        Color color = data.ColorFn(0f);
        Vector2 vertexOffset = positions[0]
            .DirectionTo(positions[1])
            .RotatedBy(MathHelper.PiOver2) * data.WidthFn(0f) * 0.5f;

        _trailVertices[0] = new VertexPositionColorTexture((positions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
        _trailVertices[1] = new VertexPositionColorTexture((positions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

        for(var j = 1; j < positions.Length; j++) {
            var factor = j / (positions.Length - 1f);

            color = data.ColorFn(factor);

            var currentPosition = positions[j];
            var previousPosition = positions[j - 1];

            vertexOffset =
                previousPosition.DirectionTo(currentPosition).RotatedBy(MathHelper.PiOver2) * data.WidthFn(factor) * 0.5f;

            _trailVertices[j * 2] = new VertexPositionColorTexture(
                (currentPosition - vertexOffset).ToVector3(),
                color,
                new(factor, 0f)
            );
            _trailVertices[j * 2 + 1] = new VertexPositionColorTexture(
                (currentPosition + vertexOffset).ToVector3(),
                color,
                new(factor, 1f)
            );

            _trailIndices[(j - 1) * 6] = (ushort)((j - 1) * 2);
            _trailIndices[(j - 1) * 6 + 1] = (ushort)((j - 1) * 2 + 2);
            _trailIndices[(j - 1) * 6 + 2] = (ushort)((j - 1) * 2 + 3);
            _trailIndices[(j - 1) * 6 + 3] = (ushort)((j - 1) * 2 + 3);
            _trailIndices[(j - 1) * 6 + 4] = (ushort)((j - 1) * 2 + 1);
            _trailIndices[(j - 1) * 6 + 5] = (ushort)((j - 1) * 2);
        }

        _trailVertexBuffer.SetData(_trailVertices);
        Device.SetVertexBuffer(_trailVertexBuffer);

        _trailIndexBuffer.SetData(_trailIndices);
        Device.Indices = _trailIndexBuffer;

        TrailEffect.Parameters["uMatrix"].SetValue(_matrix);
        TrailEffect.Parameters["uSpriteRotation"].SetValue(data.SpriteRotation);
        TrailEffect.CurrentTechnique.Passes[0].Apply();

        if(data.Effect is not null) {
            foreach(var pass in data.Effect.CurrentTechnique.Passes) {
                pass.Apply();

                Device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    positions.Length * 2,
                    0,
                    (positions.Length - 1) * 2
                );
            }

            return;
        }

        Device.DrawIndexedPrimitives(
            PrimitiveType.TriangleList,
            0,
            0,
            positions.Length * 2,
            0,
            (positions.Length - 1) * 2
        );
    }

    void ExecuteApplyEffect(int dataIndex) {
        var effect = _commands.Effects[dataIndex];

        var currentViewPort = Device.Viewport;
        var currentBlendState = Device.BlendState;

        (_swapTarget, _drawTarget) = (_drawTarget!, _swapTarget);
        Device.SetRenderTarget(_drawTarget);
        Device.Clear(Color.Transparent);
        Device.BlendState = BlendState.AlphaBlend;

        DrawQuad(
            _swapTarget,
            [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)],
            new(0, 0, 1, 1),
            Color.White,
            Matrix.Identity,
            effect);

        Device.Viewport = currentViewPort;
        Device.BlendState = currentBlendState;
    }

    void ExecuteClear(int dataIndex) {
        var color = _commands.Colors[dataIndex];
        Device.Clear(color);
    }

    void ExecuteSetTexture(int dataIndex) {
        var data = _commands.SetTextureData[dataIndex];
        Device.Textures[data.Index] = _commands.Textures[data.TextureIndex];
    }

    void ExecuteSetSamplerState(int dataIndex) {
        var data = _commands.SetSamplerStateData[dataIndex];
        Device.SamplerStates[data.Index] = _commands.SamplerStates[data.SamplerStateIndex];
    }

    void ExecuteSetBlendState(int dataIndex) {
        Device.BlendState = _commands.BlendStates[dataIndex];
    }

    void ExecuteSetEffectParams(int dataIndex) {
        var data = _commands.SetEffectParamsData[dataIndex];
        var parameters =
            CollectionsMarshal.AsSpan(_commands.EffectParams)[data.EffectParamsIndex..(data.EffectParamsIndex + data.EffectParamCount)];

        foreach(var (name, value) in parameters) {
            var parameter = data.Effect.Parameters[name];
            switch(value.Type) {
                case ParameterValueType.Int:
                    parameter.SetValue(value.Int);
                    break;
                case ParameterValueType.Float:
                    parameter.SetValue(value.Float);
                    break;
                case ParameterValueType.Vector2:
                    parameter.SetValue(value.Vector2);
                    break;
                case ParameterValueType.Vector3:
                    parameter.SetValue(value.Vector3);
                    break;
                case ParameterValueType.Vector4:
                    parameter.SetValue(value.Vector4);
                    break;
                case ParameterValueType.Texture2D:
                    parameter.SetValue(value.Texture2D);
                    break;
                case ParameterValueType.Matrix:
                    parameter.SetValue(value.Matrix);
                    break;
            }
        }
    }

    void DrawQuad(
        Texture2D texture,
        ReadOnlySpan<Vector2> positions,
        Vector4 source,
        Color color,
        Matrix matrix,
        Effect? effect
    ) {
        _quadVertexBuffer.SetData<VertexPositionColorTexture>([
            new(positions[0].ToVector3(), color, new(source.X + source.Z, source.Y)),
            new(positions[1].ToVector3(), color, new(source.X + source.Z, source.Y + source.W)),
            new(positions[2].ToVector3(), color, new(source.X, source.Y)),
            new(positions[3].ToVector3(), color, new(source.X, source.Y + source.W))]); // meh

        Device.SetVertexBuffer(_quadVertexBuffer);
        Device.Indices = null;

        QuadEffect.Parameters["uMatrix"].SetValue(matrix);
        QuadEffect.CurrentTechnique.Passes[0].Apply();

        if(effect is not null) {
            foreach(var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();

                Device.Textures[0] = texture;
                Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            return;
        }

        Device.Textures[0] = texture;
        Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    static RenderTarget2D CreateRenderTarget() => new(
        Device,
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 2,
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 2,
        false,
        SurfaceFormat.Color,
        DepthFormat.None,
        0,
        RenderTargetUsage.PreserveContents
    );

    struct RenderCommands() {
        public List<CommandTag> Tags = [];
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

        public readonly void Clear() {
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
    }

    record struct BeginData(
        float Scale,
        int MatrixIndex
    );

    record struct DrawTextureData(
        Texture2D Texture,
        Color Color,
        int PositionDataIndex,
        Vector4 Source,
        Effect? Effect
    );

    record struct DrawTrailData(
        int PositionsIndex,
        int PositionCount,
        Func<float, float> WidthFn,
        Func<float, Color> ColorFn,
        Effect? Effect,
        int SpriteRotation
    );

    record struct SetTextureData(
        int Index,
        int TextureIndex
    );

    record struct SetSamplerState(
        int Index,
        int SamplerStateIndex
    );

    record struct SetEffectParamsData(
        Effect Effect,
        int EffectParamsIndex,
        int EffectParamCount
    );

    enum CommandTag : byte {
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

    struct VertexId(float id) : IVertexType {
        public float Id = id;

        public static readonly VertexDeclaration VertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0));

        readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}

[StructLayout(LayoutKind.Explicit)]
internal struct EffectParameterValue {
    [FieldOffset(0)]
    public ParameterValueType Type;

    // References cannot be overlapped unfortunately..
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
        Type = ParameterValueType.Float,
        Float = value,
    };

    public static implicit operator EffectParameterValue(int value) => new()
    {
        Type = ParameterValueType.Int,
        Int = value,
    };

    public static implicit operator EffectParameterValue(Vector2 value) => new()
    {
        Type = ParameterValueType.Vector2,
        Vector2 = value,
    };

    public static implicit operator EffectParameterValue(Vector3 value) => new()
    {
        Type = ParameterValueType.Vector3,
        Vector3 = value,
    };

    public static implicit operator EffectParameterValue(Vector4 value) => new()
    {
        Type = ParameterValueType.Vector4,
        Vector4 = value,
    };

    public static implicit operator EffectParameterValue(Texture2D value) => new()
    {
        Type = ParameterValueType.Texture2D,
        Texture2D = value,
    };

    public static implicit operator EffectParameterValue(Matrix value) => new()
    {
        Type = ParameterValueType.Matrix,
        Matrix = value,
    };

    public static implicit operator EffectParameterValue(Color value) => new()
    {
        Type = ParameterValueType.Vector4,
        Vector4 = value.ToVector4(),
    };
}

internal enum ParameterValueType {
    Float,
    Int,
    Vector2,
    Vector3,
    Vector4,
    Texture2D,
    Matrix,
}
