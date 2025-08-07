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

using DataIndex = int;
using EffectDataIndex = int;

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

public enum RuntimeParameterValue {
    TargetSize,
    WorldToTargetMatrix,
    ScreenToTargetMatrix,
}

public class Renderer : ModSystem {
    struct Commands() {
        public List<CommandType> Types = [];
        public List<DataIndex> Datas = [];

        public readonly int Count => Types.Count;
        public readonly void Add(CommandType type, DataIndex data) {
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
        DrawSpritePosition,
        DrawSpriteRectangle,

        Begin,
        End,

        ApplyEffect,
        EffectParams,
    }


    struct DrawSpritePositionData {
        public Texture2D Texture;
        public Vector2 Position;
        public Color Color;
        public Rectangle? Source;
        public float Rotation;
        public Vector2 Origin;
        public Vector2 Scale;
        public SpriteEffects SpriteEffects;
    }

    struct DrawSpriteRectangleData {
        public Texture2D Texture;
        public Rectangle Destination;
        public Color Color;
        public Rectangle? Source;
        public float Rotation;
        public Vector2 Origin;
        public SpriteEffects SpriteEffects;
    }

    struct DrawTrailData {
        public int PositionsIndex;
        public int PositionCount;
        public Func<float, float> Width;
        public Func<float, Color> Color;
        public EffectDataIndex EffectData;
    }

    struct BeginData {
        public float Scale;
        public DataIndex SnapshotIndex;
    }

    struct EffectData {
        public Effect Effect;
        public int ParameterIndex;
        public int ParameterCount;
    }

    struct EffectParameter {
        public string Name;
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

        [FieldOffset(16)]
        public RuntimeParameterValue RuntimeValue;

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

        public static implicit operator ParameterValue(RuntimeParameterValue value) => new()
        {
            Type = ParameterValueType.RuntimeValue,
            RuntimeValue = value,
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
        RuntimeValue,
    }

    static readonly List<EffectParameter> _effectParameters = [];

    static readonly List<DrawSpritePositionData> _spritePositionDatas = [];
    static readonly List<DrawSpriteRectangleData> _spriteRectangleDatas = [];
    static readonly List<DrawTrailData> _trailDatas = [];
    static readonly List<SpriteBatchSnapshot> _snapshotDatas = [];
    static readonly List<BeginData> _beginDatas = [];
    static readonly List<EffectData> _effectDatas = [];
    static readonly List<Vector2> _trailPositions = [];

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

    static RenderTarget2D InitFullScreenTarget => new(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            _trailVertexBuffer = new DynamicVertexBuffer(
                Main.graphics.GraphicsDevice,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly
            );
            _trailIndexBuffer = new DynamicIndexBuffer(
                Main.graphics.GraphicsDevice,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly
            );

            _activeTarget = InitFullScreenTarget;
            _inactiveTarget = InitFullScreenTarget;
            _targetSemaphore.Release();
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
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
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

    public override void PreUpdateEntities() {
        _effectParameters.Clear();

        _spritePositionDatas.Clear();
        _spriteRectangleDatas.Clear();
        _trailDatas.Clear();
        _snapshotDatas.Clear();
        _beginDatas.Clear();
        _effectDatas.Clear();
        _trailPositions.Clear();

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
        _beginDatas.Add(new() { Scale = Math.Clamp(scale, 0f, 1f), SnapshotIndex = snapshotIndex });

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
                ("size", RuntimeParameterValue.TargetSize)
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

            var trailPositionsIndex = _trailPositions.Count;
            _trailPositions.AddRange(positions);

            var trailDataIndex = _trailDatas.Count;
            _trailDatas.Add(new()
            {
                PositionsIndex = trailPositionsIndex,
                PositionCount = positions.Length,
                Width = width,
                Color = color,
                EffectData = effectDataIndex,
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
                ("transformationMatrix", RuntimeParameterValue.WorldToTargetMatrix),
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
                ("transformationMatrix", RuntimeParameterValue.WorldToTargetMatrix),
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

        static EffectDataIndex AddEffectData(Effect effect, ReadOnlySpan<(string, ParameterValue)> parameters) {
            var parameterIndex = _effectParameters.Count;
            var parameterCount = parameters.Length;
            foreach(var (name, value) in parameters) {
                _effectParameters.Add(new()
                {
                    Name = name,
                    Value = value,
                });
            }

            var index = _effectDatas.Count;
            _effectDatas.Add(new()
            {
                Effect = effect,
                ParameterIndex = parameterIndex,
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
            var index = _spritePositionDatas.Count;
            _spritePositionDatas.Add(new()
            {
                Texture = texture,
                Position = position,
                Color = color ?? Color.White,
                Source = source,
                Rotation = rotation,
                Origin = origin ?? Vector2.Zero,
                Scale = scale ?? Vector2.One,
                SpriteEffects = spriteEffects,
            });
            _cache.Add(CommandType.DrawSpritePosition, index);
            return this;
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
            var index = _spriteRectangleDatas.Count;
            _spriteRectangleDatas.Add(new()
            {
                Texture = texture,
                Destination = destination,
                Color = color ?? Color.White,
                Source = source,
                Rotation = rotation,
                Origin = origin ?? Vector2.Zero,
                SpriteEffects = spriteEffects,
            });
            _cache.Add(CommandType.DrawSpriteRectangle, index);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    // TODO: put this in a separate file and expose renderer data.
    struct CommandRunner {
        float _targetScale;

        Matrix _worldToTargetMatrix;
        Matrix _screenToTargetMatrix;

        RenderTargetBinding[] _cachedBindings;
        RenderTargetUsage _cachedUsage;

        public static void Run(in Commands commands) {
            _targetSemaphore.WaitOne();
            var r = new CommandRunner();

            SpriteBatchSnapshot? snapshot = null;
            if(Main.spriteBatch.beginCalled) {
                Main.spriteBatch.End(out var snap);
                snapshot = snap;
            }

            for(var i = 0; i < commands.Count; i++) {
                var dataIndex = commands.Datas[i];
                switch(commands.Types[i]) {
                    case CommandType.DrawTrail:
                        r.RunDrawTrail(dataIndex);
                        break;
                    case CommandType.DrawSpritePosition:
                        r.RunDrawSpritePosition(dataIndex);
                        break;
                    case CommandType.DrawSpriteRectangle:
                        r.RunDrawSpriteRectangle(dataIndex);
                        break;
                    case CommandType.Begin:
                        r.RunBegin(dataIndex);
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

            if(snapshot is { } s) Main.spriteBatch.Begin(s);
            _targetSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawTrail(DataIndex index) {
            var trailData = _trailDatas[index];
            var trailPositions = CollectionsMarshal
                .AsSpan(_trailPositions)[trailData.PositionsIndex..(trailData.PositionsIndex + trailData.PositionCount)];

            Color color = trailData.Color(0f);
            Vector2 vertexOffset = trailPositions[0]
                .DirectionTo(trailPositions[1])
                .RotatedBy(MathHelper.PiOver2) * trailData.Width(0f) * 0.5f;

            _trailVertices[0] = new VertexPositionColorTexture((trailPositions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
            _trailVertices[1] = new VertexPositionColorTexture((trailPositions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

            for(var j = 1; j < trailPositions.Length; j++) {
                var factor = j / (trailPositions.Length - 1f);

                color = trailData.Color(factor);
                vertexOffset = trailPositions[j - 1]
                    .DirectionTo(trailPositions[j])
                    .RotatedBy(MathHelper.PiOver2)
                    * trailData.Width(factor) * 0.5f;

                _trailVertices[j * 2] = new VertexPositionColorTexture(
                    (trailPositions[j] - vertexOffset).ToVector3(),
                    color,
                    new(factor, 0f)
                );
                _trailVertices[j * 2 + 1] = new VertexPositionColorTexture(
                    (trailPositions[j] + vertexOffset).ToVector3(),
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
            Main.graphics.GraphicsDevice.SetVertexBuffer(_trailVertexBuffer);

            _trailIndexBuffer.SetData(_trailIndices);
            Main.graphics.GraphicsDevice.Indices = _trailIndexBuffer;

            var effectData = _effectDatas[trailData.EffectData];
            SetEffectParams(effectData);

            foreach(EffectPass pass in effectData.Effect.CurrentTechnique.Passes) {
                pass.Apply();
                Main.graphics.GraphicsDevice.DrawIndexedPrimitives(
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
        readonly void RunDrawSpritePosition(DataIndex index) {
            var spriteData = _spritePositionDatas[index];
            Main.spriteBatch.Draw(
                spriteData.Texture,
                spriteData.Position,
                spriteData.Source,
                spriteData.Color,
                spriteData.Rotation,
                spriteData.Origin,
                spriteData.Scale,
                spriteData.SpriteEffects,
                0f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunDrawSpriteRectangle(DataIndex index) {
            var rectangleData = _spriteRectangleDatas[index];
            Main.spriteBatch.Draw(
                rectangleData.Texture,
                rectangleData.Destination,
                rectangleData.Source,
                rectangleData.Color,
                rectangleData.Rotation,
                rectangleData.Origin,
                rectangleData.SpriteEffects,
                0f
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunBegin(DataIndex index) {
            var beginData = _beginDatas[index];

            _targetScale = beginData.Scale;
            var snapshot = _snapshotDatas[beginData.SnapshotIndex];

            _screenToTargetMatrix = Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            _worldToTargetMatrix = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * _screenToTargetMatrix;

            _cachedBindings = Main.graphics.GraphicsDevice.GetRenderTargets();
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;
                ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
            }

            Main.graphics.graphicsDevice.SetRenderTarget(_activeTarget);
            SetTargetViewport();

            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(snapshot with
            {
                TransformMatrix = snapshot.TransformMatrix * Matrix.CreateScale(_targetScale)
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RunApplyEffect(DataIndex index) {
            var effectData = _effectDatas[index];

            (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
            Main.graphics.GraphicsDevice.SetRenderTarget(_activeTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            SetEffectParams(effectData);
            var snapshot = Main.spriteBatch.CaptureEndBegin(new()
            {
                CustomEffect = effectData.Effect,
                TransformMatrix = Matrix.Identity,
            });

            Main.spriteBatch.Draw(_inactiveTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.EndBegin(snapshot);
            SetTargetViewport();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEffectParams(DataIndex index) {
            SetEffectParams(_effectDatas[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void RunEnd(DataIndex _) {
            Main.graphics.GraphicsDevice.SetRenderTargets(_cachedBindings);
            if(_cachedBindings != null && _cachedBindings.Length > 0) {
                ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage = _cachedUsage;
            }

            Main.spriteBatch.EndBegin(new()
            {
                TransformMatrix = Matrix.CreateScale(1f / _targetScale),
            });
            Main.spriteBatch.Draw(_activeTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.White);

            // This fixes the issue with vanilla trail being drawn 2x bigger in case of half size target..
            // The spritebatch sets the transformation matrix in `End`
            // and the trails depend on it so it needs to be set back to normal.
            Main.spriteBatch.EndBegin(new());
            Main.spriteBatch.End();
        }

        readonly void SetTargetViewport() {
            Main.graphics.GraphicsDevice.Viewport = new(
                0,
                0,
                (int)(Main.screenWidth * _targetScale),
                (int)(Main.screenHeight * _targetScale)
            );
        }

        readonly void SetEffectParams(EffectData effectData) {
            var effect = effectData.Effect;
            for(var j = 0; j < effectData.ParameterCount; j++) {
                var parameter = _effectParameters[j + effectData.ParameterIndex];
                switch(parameter.Value.Type) {
                    case ParameterValueType.Int:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Int);
                        break;
                    case ParameterValueType.Float:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Float);
                        break;
                    case ParameterValueType.Vector2:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Vector2);
                        break;
                    case ParameterValueType.Vector3:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Vector3);
                        break;
                    case ParameterValueType.Vector4:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Vector4);
                        break;
                    case ParameterValueType.Texture2D:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Texture2D);
                        break;
                    case ParameterValueType.Matrix:
                        effect.Parameters[parameter.Name].SetValue(parameter.Value.Matrix);
                        break;
                    case ParameterValueType.RuntimeValue:
                        switch(parameter.Value.RuntimeValue) {
                            case RuntimeParameterValue.TargetSize:
                                effect.Parameters[parameter.Name].SetValue(Main.ScreenSize.ToVector2());
                                break;
                            case RuntimeParameterValue.WorldToTargetMatrix:
                                effect.Parameters[parameter.Name].SetValue(_worldToTargetMatrix);
                                break;
                            case RuntimeParameterValue.ScreenToTargetMatrix:
                                effect.Parameters[parameter.Name].SetValue(_screenToTargetMatrix);
                                break;
                        }

                        break;
                }
            }
        }
    }
}
