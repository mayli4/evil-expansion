using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

public enum RenderLayer {
    BeforeTiles,
    AfterTiles,
    BeforeProjectiles,
    AfterProjectiles,
    BeforeNPCs,
    AfterNPCs,
    BeforePlayers,
    AfterPlayers,
}

public class Graphics : ModSystem {
    struct Commands() {
        public List<CommandType> Types = [];
        public List<int> Datas = [];

        public readonly int Count => Types.Count;
        public readonly void Add(CommandType type, int data) {
            Types.Add(type);
            Datas.Add(data);
        }

        public readonly void AddRange(in Commands commands) {
            Types.AddRange(commands.Types);
            Datas.AddRange(commands.Datas);
        }

        public readonly void Clear() {
            Types.Clear();
            Datas.Clear();
        }
    }

    enum CommandType : byte {
        DrawTrail,
        DrawSprite,

        Begin,
        Reset,
        End,

        ApplyEffect,
        EffectParams,
    }

    struct DrawSpriteData {
        public Texture2D Texture;
        public Color Color;

        // 4 x [Vertex Positions, Texture Coordinates] in _positionDatas
        public int PositionDatasIndex;
    }

    struct DrawTrailData {
        public int PositionDatasIndex;
        public int PositionCount;
        public Func<float, float> Width;
        public Func<float, Color> Color;
        public int EffectDatasIndex;
    }

    struct BeginData {
        public float Scale;
        public int SnapshotDatasIndex;
    }

    struct EffectData {
        public Effect Effect;
        public int ParameterDatasIndex;
        public int ParameterCount;
    }

    struct EffectParameterData {
        public int Index;
        public ParameterValue Value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ParameterValue {
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

        public static implicit operator ParameterValue(float value) => new()
        {
            Type = ParameterValueType.Float,
            Float = value,
        };

        public static implicit operator ParameterValue(int value) => new()
        {
            Type = ParameterValueType.Int,
            Int = value,
        };

        public static implicit operator ParameterValue(Vector2 value) => new()
        {
            Type = ParameterValueType.Vector2,
            Vector2 = value,
        };

        public static implicit operator ParameterValue(Vector3 value) => new()
        {
            Type = ParameterValueType.Vector3,
            Vector3 = value,
        };

        public static implicit operator ParameterValue(Vector4 value) => new()
        {
            Type = ParameterValueType.Vector4,
            Vector4 = value,
        };

        public static implicit operator ParameterValue(Texture2D value) => new()
        {
            Type = ParameterValueType.Texture2D,
            Texture2D = value,
        };

        public static implicit operator ParameterValue(Matrix value) => new()
        {
            Type = ParameterValueType.Matrix,
            Matrix = value,
        };
    }

    public enum ParameterValueType {
        Float,
        Int,
        Vector2,
        Vector3,
        Vector4,
        Texture2D,
        Matrix,
    }

    public static Matrix WorldTransformMatrix { get; private set; }
    public static Matrix ScreenTransformMatrix { get; private set; }

    static readonly List<EffectParameterData> _effectParameters = [];

    static readonly List<DrawSpriteData> _spriteDatas = [];
    static readonly List<DrawTrailData> _trailDatas = [];
    static readonly List<SpriteBatchSnapshot> _snapshotDatas = [];
    static readonly List<BeginData> _beginDatas = [];
    static readonly List<EffectData> _effectDatas = [];
    static readonly List<Vector2> _positionDatas = [];

    static Commands _cache = new();

    static Commands _beforeTiles = new();
    static Commands _afterTiles = new();
    static Commands _beforeProjectiles = new();
    static Commands _afterProjectiles = new();
    static Commands _beforeNPCs = new();
    static Commands _afterNPCs = new();
    static Commands _beforePlayers = new();
    static Commands _afterPlayers = new();

    const int TrailPositionCapacity = 256;
    const int TrailVertexCount = TrailPositionCapacity * 2;
    const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    static DynamicVertexBuffer _trailVertexBuffer;
    static readonly VertexPositionColorTexture[] _trailVertices = new VertexPositionColorTexture[TrailVertexCount];

    static DynamicIndexBuffer _trailIndexBuffer;
    static readonly ushort[] _trailIndices = new ushort[TrailIndexCount];

    readonly static Semaphore _targetSemaphore = new(0, 1);
    static RenderTarget2D _activeTarget;
    static RenderTarget2D _inactiveTarget;

    static GraphicsDevice GraphicsDevice => Main.graphics.GraphicsDevice;
    static SpriteBatch SpriteBatch => Main.spriteBatch;
    static RenderTarget2D InitFullScreenTarget => new(GraphicsDevice, Main.screenWidth, Main.screenHeight);

    static Effect _spriteEffect;
    static nint _spriteMatrix;

    static VertexBuffer _spriteVertexBuffer;
    static IndexBuffer _spriteIndexBuffer;

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            _trailVertexBuffer = new DynamicVertexBuffer(
                GraphicsDevice,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly
            );
            _trailIndexBuffer = new DynamicIndexBuffer(
                GraphicsDevice,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly
            );

            _activeTarget = InitFullScreenTarget;
            _inactiveTarget = InitFullScreenTarget;
            _targetSemaphore.Release();

            _spriteVertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            _spriteIndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            _spriteIndexBuffer.SetData(new ushort[] { 0, 1, 2, 3, 2, 1 });

            _spriteEffect = new Effect(GraphicsDevice, Resources.SpriteEffect);
            _spriteMatrix = _spriteEffect.Parameters["MatrixTransform"].values;
        });

        Main.OnResolutionChanged += (screenSize) =>
        {
            Main.QueueMainThreadAction(() =>
            {
                _targetSemaphore.WaitOne();

                _activeTarget.Dispose();
                _inactiveTarget.Dispose();

                _activeTarget = InitFullScreenTarget;
                _inactiveTarget = InitFullScreenTarget;

                _targetSemaphore.Release();
            });
        };

        On_Main.DrawNPCs += On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles += On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles += On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs += On_Main_DrawCachedProjs;
    }

    public override void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        On_Main.DrawSuperSpecialProjectiles -= On_Main_DrawSuperSpecialProjectiles;
        On_Main.DrawPlayers_AfterProjectiles -= On_Main_DrawPlayers_AfterProjectiles;
        On_Main.DrawCachedProjs -= On_Main_DrawCachedProjs;

        Main.QueueMainThreadAction(() =>
        {
            _activeTarget.Dispose();
            _inactiveTarget.Dispose();

            _spriteVertexBuffer.Dispose();
            _spriteIndexBuffer.Dispose();
            _spriteEffect.Dispose();
        });
    }

    private void On_Main_DrawSuperSpecialProjectiles(On_Main.orig_DrawSuperSpecialProjectiles orig, Main self, List<int> projCache, bool startSpriteBatch) {
        CommandRunner.Run(in _beforeProjectiles);
        orig(self, projCache, startSpriteBatch);
    }

    private void On_Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) {
        orig(self, projCache, startSpriteBatch);
        CommandRunner.Run(in _afterProjectiles);
    }

    private void On_Main_DrawPlayers_AfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        CommandRunner.Run(in _beforePlayers);
        orig(self);
        CommandRunner.Run(in _afterPlayers);

        PostDraw();
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            PreDraw();
            CommandRunner.Run(in _beforeTiles);
            orig(self, behindTiles);
        }
        else {
            CommandRunner.Run(in _afterTiles);
            CommandRunner.Run(in _beforeNPCs);
            orig(self, behindTiles);
            CommandRunner.Run(in _afterNPCs);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void PreDraw() {
        ScreenTransformMatrix =
            Main.GameViewMatrix.TransformationMatrix
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
        WorldTransformMatrix =
            Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
            * ScreenTransformMatrix;
    }

    static void PostDraw() {
        _effectParameters.Clear();

        _spriteDatas.Clear();
        _trailDatas.Clear();
        _snapshotDatas.Clear();
        _beginDatas.Clear();
        _effectDatas.Clear();
        _positionDatas.Clear();

        _beforeTiles.Clear();
        _afterTiles.Clear();
        _beforeProjectiles.Clear();
        _afterProjectiles.Clear();
        _beforeNPCs.Clear();
        _afterNPCs.Clear();
        _beforePlayers.Clear();
        _afterPlayers.Clear();
    }

    public static Pipeline BeginPipeline(float scale = 1f, SpriteBatchSnapshot? snapshot = null) {
        if(_cache.Count != 0) throw new Exception("One pipeline can be begun at a time.");

        var snapshotIndex = _snapshotDatas.Count;
        _snapshotDatas.Add(snapshot ?? new());

        var beginDataIndex = _beginDatas.Count;
        _beginDatas.Add(new() { Scale = Math.Clamp(scale, 0f, 1f), SnapshotDatasIndex = snapshotIndex });

        _cache.Add(CommandType.Begin, beginDataIndex);
        return new();
    }

    // TODO: Place project specific methods in an extension class (ApplyOutline, DrawBasicTrail, etc.).
    public struct Pipeline {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline ApplyOutline(Color color) {
            ApplyEffect(
                Assets.Assets.Effects.Pixel.Outline.Value,
                ("color", color.ToVector4()),
                ("size", Main.ScreenSize.ToVector2())
            );
            return this;
        }

        public readonly Pipeline DrawTrail(
            ReadOnlySpan<Vector2> positions,
            Func<float, float> width,
            Func<float, Color> color,
            Effect effect,
            params ReadOnlySpan<(string, ParameterValue)> parameters
        ) {
            var effectDataIndex = AddEffectData(effect, parameters);

            var trailPositionsIndex = _positionDatas.Count;
            _positionDatas.AddRange(positions);

            var trailDataIndex = _trailDatas.Count;
            _trailDatas.Add(new()
            {
                PositionDatasIndex = trailPositionsIndex,
                PositionCount = positions.Length,
                Width = width,
                Color = color,
                EffectDatasIndex = effectDataIndex,
            });
            _cache.Add(CommandType.DrawTrail, trailDataIndex);

            return this;
        }

        public readonly Pipeline DrawBasicTrail(
            ReadOnlySpan<Vector2> positions,
            Func<float, float> width,
            Texture2D texture,
            Color color,
            int spriteRotation = 0
        ) {
            var effect = Assets.Assets.Effects.Trail.Default.Value;
            ReadOnlySpan<(string, ParameterValue)> parameters = [
                ("sampleTexture", texture),
                ("color", color.ToVector4()),
                ("transformationMatrix", WorldTransformMatrix),
                ("spriteRotation", spriteRotation)
            ];

            return DrawTrail(positions, width, static _ => Color.White, effect, parameters);
        }

        public readonly Pipeline DrawBasicTrail(
            ReadOnlySpan<Vector2> positions,
            Func<float, float> width,
            Texture2D texture,
            Func<float, Color> color,
            int spriteRotation = 0
        ) {
            var effect = Assets.Assets.Effects.Trail.Default.Value;
            ReadOnlySpan<(string, ParameterValue)> parameters = [
                ("sampleTexture", texture),
                ("color", Color.White.ToVector4()),
                ("transformationMatrix", WorldTransformMatrix),
                ("spriteRotation", spriteRotation)
            ];

            return DrawTrail(positions, width, color, effect, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline EffectParams(
            Effect effect,
            params ReadOnlySpan<(string, ParameterValue)> parameters
        ) {
            _cache.Add(CommandType.EffectParams, AddEffectData(effect, parameters));
            return this;
        }

        static int AddEffectData(Effect effect, ReadOnlySpan<(string, ParameterValue)> parameters) {
            var parameterIndex = _effectParameters.Count;
            var parameterCount = parameters.Length;
            foreach(var (name, value) in parameters) {
                // This is literally just what 'effect.Parameters[name]' does.
                // And I feel like its better to fail here rather than when actually drawing.
                var i = 0;
                for(; i < effect.Parameters.Count; i++) {
                    if(effect.Parameters[i].Name == name) break;
                }

                if(i == effect.Parameters.Count) {
                    _cache.Clear();
                    throw new Exception($"Invalid parameter name '{name}'.");
                }

                _effectParameters.Add(new()
                {
                    Index = i,
                    Value = value,
                });
            }

            var index = _effectDatas.Count;
            _effectDatas.Add(new()
            {
                Effect = effect,
                ParameterDatasIndex = parameterIndex,
                ParameterCount = parameterCount,
            });
            return index;
        }

        public readonly Pipeline DrawSprite(
            Texture2D texture,
            Vector2 position,
            Color? color = null,
            Rectangle? source = null,
            float rotation = 0f,
            Vector2? origin = null,
            Vector2? scale = null,
            SpriteEffects spriteEffects = SpriteEffects.None
        ) {
            var actualScale = scale ?? Vector2.One;
            var actualSource = source ?? new Rectangle(0, 0, texture.Width, texture.Height);
            return DrawSprite(
                texture,
                new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)(actualSource.Width / actualScale.X),
                    (int)(actualSource.Height / actualScale.Y)
                ),
                color ?? Color.White,
                actualSource,
                rotation,
                origin ?? Vector2.Zero,
                spriteEffects
            );
        }
        public readonly Pipeline DrawSprite(
            Texture2D texture,
            Rectangle destination,
            Color? color = null,
            Rectangle? source = null,
            float rotation = 0f,
            Vector2? origin = null,
            SpriteEffects spriteEffects = SpriteEffects.None
        ) {
            return DrawSprite(
                texture,
                destination,
                color ?? Color.White,
                source ?? new Rectangle(0, 0, texture.Width, texture.Height),
                rotation,
                origin ?? Vector2.Zero,
                spriteEffects
            );
        }

        public readonly Pipeline DrawSprite(
            Texture2D texture,
            Rectangle destination,
            Color color,
            Rectangle source,
            float rotation,
            Vector2 origin,
            SpriteEffects spriteEffects
        ) {
            var rCos = MathF.Cos(rotation);
            var rSin = MathF.Sin(rotation);

            var sourceWidthNormalized =
                Math.Sign(source.Width) * MathF.Max(MathF.Abs(source.Width), MathHelper.MachineEpsilonFloat) / texture.Width;
            var sourceHeightNormalized =
                Math.Sign(source.Height) * MathF.Max(MathF.Abs(source.Height), MathHelper.MachineEpsilonFloat) / texture.Height;

            var originXNormalized = origin.X / sourceWidthNormalized / texture.Width;
            var originYNormalized = origin.Y / sourceHeightNormalized / texture.Height;

            Span<Vector2> positions = stackalloc Vector2[8];

            var originOffset = new Vector2(
                -originXNormalized * destination.Width,
                -originYNormalized * destination.Height
            );
            positions[0] = new(
                -rSin * originOffset.Y + rCos * originOffset.X + destination.X,
                rCos * originOffset.Y + rSin * originOffset.X + destination.Y
            );

            originOffset = new Vector2(
                (1f - originXNormalized) * destination.Width,
                -originYNormalized * destination.Height
            );
            positions[1] = new(
                -rSin * originOffset.Y + rCos * originOffset.X + destination.X,
                rCos * originOffset.Y + rSin * originOffset.X + destination.Y
            );

            originOffset = new Vector2(
                -originXNormalized * destination.Width,
                (1f - originYNormalized) * destination.Height
            );
            positions[2] = new(
                -rSin * originOffset.Y + rCos * originOffset.X + destination.X,
                rCos * originOffset.Y + rSin * originOffset.X + destination.Y
            );

            originOffset = new Vector2(
                (1f - originXNormalized) * destination.Width,
                (1f - originYNormalized) * destination.Height
            );
            positions[3] = new(
                -rSin * originOffset.Y + rCos * originOffset.X + destination.X,
                rCos * originOffset.Y + rSin * originOffset.X + destination.Y
            );

            ReadOnlySpan<float> cornerOffsetX = [0f, 1f, 0f, 1f];
            ReadOnlySpan<float> cornerOffsetY = [0f, 0f, 1f, 1f];

            var effects = (byte)(spriteEffects & (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically));

            var sourceXNormalized = source.X / texture.Width;
            var sourceYNormalized = source.Y / texture.Width;

            for(byte i = 0; i < 4; i++) {
                positions[i + 4] = new(
                    cornerOffsetX[i ^ effects] * sourceXNormalized + sourceWidthNormalized,
                    cornerOffsetY[i ^ effects] * sourceYNormalized + sourceHeightNormalized
                );
            }

            var positionDatasIndex = _positionDatas.Count;
            _positionDatas.AddRange(positions);

            var spriteDatasIndex = _spriteDatas.Count;
            _spriteDatas.Add(new()
            {
                Texture = texture,
                Color = color,
                PositionDatasIndex = positionDatasIndex,
            });
            _cache.Add(CommandType.DrawSprite, spriteDatasIndex);
            return this;
        }

        public readonly Pipeline ApplyTint(Color color) {
            ApplyEffect(Assets.Assets.Effects.Pixel.Tint.Value, ("color", color.ToVector4()));
            return this;
        }

        public readonly Pipeline ApplyEffect(Effect effect, params ReadOnlySpan<(string, ParameterValue)> parameters) {
            var effectDataIndex = AddEffectData(effect, parameters);
            _cache.Add(CommandType.ApplyEffect, effectDataIndex);
            return this;
        }

        public readonly Pipeline Reset(float scale = 1f, SpriteBatchSnapshot? snapshot = null) {
            var snapshotIndex = _snapshotDatas.Count;
            _snapshotDatas.Add(snapshot ?? new());

            var beginDataIndex = _beginDatas.Count;
            _beginDatas.Add(new() { Scale = Math.Clamp(scale, 0f, 1f), SnapshotDatasIndex = snapshotIndex });

            _cache.Add(CommandType.Reset, beginDataIndex);
            return this;
        }

        public readonly void Flush() {
            _cache.Add(CommandType.End, -1);
            CommandRunner.Run(in _cache);
            _cache.Clear();
        }

        public readonly void Schedule(RenderLayer layer) {
            _cache.Add(CommandType.End, -1);
            switch(layer) {
                case RenderLayer.BeforeTiles:
                    _beforeTiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterTiles:
                    _afterTiles.AddRange(in _cache);
                    break;
                case RenderLayer.BeforeProjectiles:
                    _beforeProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterProjectiles:
                    _afterProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.BeforeNPCs:
                    _beforeNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.AfterNPCs:
                    _afterNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.BeforePlayers:
                    _beforePlayers.AddRange(in _cache);
                    break;
                case RenderLayer.AfterPlayers:
                    _afterPlayers.AddRange(in _cache);
                    break;
            }

            _cache.Clear();
        }
    }

    struct CommandRunner {
        float _targetScale;

        RenderTargetBinding[] _cachedBindings;
        RenderTargetUsage _cachedUsage;

        public static void Run(in Commands commands) {
            _targetSemaphore.WaitOne();
            var r = new CommandRunner();

            SpriteBatchSnapshot? snapshot = null;
            if(SpriteBatch.beginCalled) {
                SpriteBatch.End(out var snap);
                snapshot = snap;
            }

            for(var i = 0; i < commands.Count; i++) {
                var dataIndex = commands.Datas[i];
                switch(commands.Types[i]) {
                    case CommandType.DrawTrail:
                        r.RunDrawTrail(dataIndex);
                        break;
                    case CommandType.DrawSprite:
                        r.RunDrawSprite(dataIndex);
                        break;
                    case CommandType.Begin:
                        r.RunBegin(dataIndex);
                        break;
                    case CommandType.Reset:
                        r.RunReset(dataIndex);
                        break;
                    case CommandType.End:
                        r.RunEnd(dataIndex);
                        break;
                    case CommandType.ApplyEffect:
                        r.RunApplyEffect(dataIndex);
                        break;
                    case CommandType.EffectParams:
                        r.RunEffectParams(dataIndex);
                        break;
                }
            }

            if(snapshot is { } s) SpriteBatch.Begin(s);
            _targetSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawTrail(int index) {
            var data = _trailDatas[index];
            var trailPositions =
                CollectionsMarshal.AsSpan(_positionDatas)[data.PositionDatasIndex..(data.PositionDatasIndex + data.PositionCount)];

            var color = data.Color(0f);
            var vertexOffset = trailPositions[0]
                .DirectionTo(trailPositions[1])
                .RotatedBy(MathHelper.PiOver2) * data.Width(0f) * 0.5f;

            _trailVertices[0] = new VertexPositionColorTexture((trailPositions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
            _trailVertices[1] = new VertexPositionColorTexture((trailPositions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

            for(var j = 1; j < trailPositions.Length; j++) {
                var factor = j / (trailPositions.Length - 1f);

                color = data.Color(factor);

                var currentPosition = trailPositions[j];
                var previousPosition = trailPositions[j - 1];

                vertexOffset = previousPosition.DirectionTo(currentPosition).RotatedBy(MathHelper.PiOver2) * data.Width(factor) * 0.5f;

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
            GraphicsDevice.SetVertexBuffer(_trailVertexBuffer);

            _trailIndexBuffer.SetData(_trailIndices);
            GraphicsDevice.Indices = _trailIndexBuffer;

            var effectData = _effectDatas[data.EffectDatasIndex];
            SetEffectParams(effectData);

            foreach(var pass in effectData.Effect.CurrentTechnique.Passes) {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    trailPositions.Length * 2,
                    0,
                    (trailPositions.Length - 1) * 2
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawSprite(int index) {
            var data = _spriteDatas[index];
            var positions = CollectionsMarshal.AsSpan(_positionDatas)[data.PositionDatasIndex..(data.PositionDatasIndex + 8)];


            _spriteVertexBuffer.SetData<VertexPositionColorTexture>([
                new(positions[0].ToVector3(), data.Color, positions[4]),
                new(positions[1].ToVector3(), data.Color, positions[5]),
                new(positions[2].ToVector3(), data.Color, positions[6]),
                new(positions[3].ToVector3(), data.Color, positions[7]),
            ]);

            GraphicsDevice.Textures[0] = data.Texture;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = Main.DefaultSamplerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.RasterizerState = Main.Rasterizer;

            GraphicsDevice.SetVertexBuffer(_spriteVertexBuffer);
            GraphicsDevice.Indices = _spriteIndexBuffer;

            unsafe {
                *(Matrix*)_spriteMatrix = ScreenTransformMatrix;
            }

            _spriteEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunApplyEffect(int index) {
            var data = _effectDatas[index];

            SetEffectParams(data);
            var snapshot = SpriteBatch.CaptureEndBegin(new()
            {
                CustomEffect = data.Effect,
                TransformMatrix = Matrix.Identity,
            });

            (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
            GraphicsDevice.SetRenderTarget(_activeTarget);
            GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch.Draw(_inactiveTarget, Vector2.Zero, Color.White);
            SpriteBatch.EndBegin(snapshot);
            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEffectParams(int index) {
            SetEffectParams(_effectDatas[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunBegin(int index) {
            var data = _beginDatas[index];
            _targetScale = data.Scale;

            _cachedBindings = GraphicsDevice.GetRenderTargets();
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;
                ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
            }

            GraphicsDevice.SetRenderTarget(_activeTarget);
            GraphicsDevice.Clear(Color.Transparent);

            var snapshot = _snapshotDatas[data.SnapshotDatasIndex];
            SpriteBatch.Begin(snapshot with
            {
                TransformMatrix = snapshot.TransformMatrix * Matrix.CreateScale(_targetScale)
            });
            SetTargetViewport();
        }

        void RunReset(int index) {
            var data = _beginDatas[index];

            var previousScale = _targetScale;
            _targetScale = data.Scale;

            SpriteBatch.EndBegin(new()
            {
                TransformMatrix = Matrix.CreateScale(_targetScale / previousScale),
            });

            (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
            GraphicsDevice.SetRenderTarget(_activeTarget);
            GraphicsDevice.Clear(Color.Transparent);

            SpriteBatch.Draw(_inactiveTarget, Vector2.Zero, Color.White);

            var snapshot = _snapshotDatas[data.SnapshotDatasIndex];
            SpriteBatch.EndBegin(snapshot with
            {
                TransformMatrix = snapshot.TransformMatrix * Matrix.CreateScale(_targetScale)
            });
            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEnd(int _) {
            SpriteBatch.EndBegin(new()
            {
                TransformMatrix = Matrix.CreateScale(
                    1f / _targetScale * Main.GameViewMatrix.Zoom.X,
                    1f / _targetScale * Main.GameViewMatrix.Zoom.Y,
                    1f
                ),
            });

            GraphicsDevice.SetRenderTargets(_cachedBindings);
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage = _cachedUsage;
            }

            SpriteBatch.Draw(_activeTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.White);

            // This fixes the issue with vanilla trail being drawn 2x bigger in case of half size target..
            // The spritebatch sets the transformation matrix in `End`
            // and the trails depend on it so it needs to be set back to normal.
            SpriteBatch.EndBegin(new());
            SpriteBatch.End();
        }

        readonly void SetTargetViewport() {
            GraphicsDevice.Viewport = new(
                0,
                0,
                (int)(Main.screenWidth * _targetScale / Main.GameViewMatrix.Zoom.X),
                (int)(Main.screenHeight * _targetScale / Main.GameViewMatrix.Zoom.Y)
            );
        }

        static void SetEffectParams(EffectData effectData) {
            var effect = effectData.Effect;
            for(var j = 0; j < effectData.ParameterCount; j++) {
                var parameterData = _effectParameters[j + effectData.ParameterDatasIndex];

                var parameter = effect.Parameters.elements[parameterData.Index];
                switch(parameterData.Value.Type) {
                    case ParameterValueType.Int:
                        parameter.SetValue(parameterData.Value.Int);
                        break;
                    case ParameterValueType.Float:
                        parameter.SetValue(parameterData.Value.Float);
                        break;
                    case ParameterValueType.Vector2:
                        parameter.SetValue(parameterData.Value.Vector2);
                        break;
                    case ParameterValueType.Vector3:
                        parameter.SetValue(parameterData.Value.Vector3);
                        break;
                    case ParameterValueType.Vector4:
                        parameter.SetValue(parameterData.Value.Vector4);
                        break;
                    case ParameterValueType.Texture2D:
                        parameter.SetValue(parameterData.Value.Texture2D);
                        break;
                    case ParameterValueType.Matrix:
                        parameter.SetValue(parameterData.Value.Matrix);
                        break;
                }
            }
        }
    }
}
