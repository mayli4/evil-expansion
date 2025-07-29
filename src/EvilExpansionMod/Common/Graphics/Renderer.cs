using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

using DataIndex = int;

public enum RenderLayer {
    BehindTiles,
    AfterTiles,
    BehindProjectiles,
    AfterProjectiles,
    BehindNPCs,
    AfterNPCs,
    BehindPlayers,
    AfterPlayers,
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
        Trail,
        Sprite,


        Begin,
        End,

        BeginPixelate,
        EndPixelate,

        ApplyOutline,
    }


    struct SpriteData {
        public Texture2D Texture;
        public Vector2 Position;
        public Color Color;
        public Rectangle? Source;
        public float Rotation;
        public Vector2 Origin;
        public Vector2 Scale;
        public SpriteEffects SpriteEffects;
    }

    struct TrailData {
        public Vector2[] Positions;
        public Func<float, float> Width;
        public Func<float, Color> Color;
        public Effect Effect;

        public int ParameterIndex;
        public int ParameterCount;
    }

    struct OutlineData {
        public Color Color;
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
        Vector2,
        Vector3,
        Vector4,
        Texture2D,
        Matrix,
    }

    public static Matrix PixelateEffectMatrix { get; private set; }


    static readonly List<EffectParameter> _effectParameters = [];

    static readonly List<SpriteData> _spriteDatas = [];
    static readonly List<TrailData> _trailDatas = [];
    static readonly List<OutlineData> _outlineDatas = [];
    static readonly List<SpriteBatchSnapshot> _beginDatas = [];

    static Commands _cache = new();

    static Commands _behindTiles = new();
    static Commands _afterTiles = new();
    static Commands _behindProjectiles = new();
    static Commands _afterProjectiles = new();
    static Commands _behindNPCs = new();
    static Commands _afterNPCs = new();
    static Commands _behindPlayers = new();
    static Commands _afterPlayers = new();

    const int TrailPositionCapacity = 256;
    const int TrailVertexCount = TrailPositionCapacity * 2;
    const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    static DynamicVertexBuffer _trailVertexBuffer;
    static readonly VertexPositionColorTexture[] _trailVertices = new VertexPositionColorTexture[TrailVertexCount];

    static DynamicIndexBuffer _trailIndexBuffer;
    static readonly ushort[] _trailIndices = new ushort[TrailIndexCount];

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
        });
    }

    public override void PreUpdateEntities() {
        _effectParameters.Clear();

        _spriteDatas.Clear();
        _trailDatas.Clear();
        _outlineDatas.Clear();
        _beginDatas.Clear();

        _behindNPCs.Clear();
        _afterTiles.Clear();
        _behindProjectiles.Clear();
        _afterProjectiles.Clear();
        _behindNPCs.Clear();
        _afterNPCs.Clear();
        _behindPlayers.Clear();
        _afterPlayers.Clear();

        PixelateEffectMatrix =
            Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
            * Matrix.CreateScale(0.5f)
            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth / 2f, Main.screenHeight / 2f, 0, -1, 1);
    }

    static void ExecuteCommands(in Commands commands) {
        SpriteBatchSnapshot? initialSnapshot = null;
        if(Main.spriteBatch.beginCalled) {
            Main.spriteBatch.End(out var snap);
            initialSnapshot = snap;
        }

        SwapTarget currentTarget = null;
        for(var i = 0; i < commands.Count; i++) {
            switch(commands.Types[i]) {
                case CommandType.Trail:
                    var trail = _trailDatas[commands.Datas[i]];

                    Color color = trail.Color(0f);
                    Vector2 vertexOffset = trail.Positions[0]
                        .DirectionTo(trail.Positions[1])
                        .RotatedBy(MathHelper.PiOver2) * trail.Width(0f) * 0.5f;

                    _trailVertices[0] = new VertexPositionColorTexture((trail.Positions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
                    _trailVertices[1] = new VertexPositionColorTexture((trail.Positions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

                    for(var j = 1; j < trail.Positions.Length; j++) {
                        var factor = j / (trail.Positions.Length - 1f);

                        color = trail.Color(factor);
                        vertexOffset = trail.Positions[j - 1]
                            .DirectionTo(trail.Positions[j])
                            .RotatedBy(MathHelper.PiOver2)
                            * trail.Width(factor) * 0.5f;

                        _trailVertices[j * 2] = new VertexPositionColorTexture(
                            (trail.Positions[j] - vertexOffset).ToVector3(),
                            color,
                            new(factor, 0f)
                        );
                        _trailVertices[j * 2 + 1] = new VertexPositionColorTexture(
                            (trail.Positions[j] + vertexOffset).ToVector3(),
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

                    var effect = trail.Effect;
                    for(var j = 0; j < trail.ParameterCount; j++) {
                        var parameter = _effectParameters[j + trail.ParameterIndex];
                        switch(parameter.Value.Type) {
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
                        }
                    }

                    foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
                        pass.Apply();
                        Main.graphics.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            trail.Positions.Length * 2,
                            0,
                            (trail.Positions.Length - 1) * 2
                        );
                    }
                    break;
                case CommandType.Sprite:
                    var sprite = _spriteDatas[commands.Datas[i]];
                    Main.spriteBatch.Draw(
                        sprite.Texture,
                        sprite.Position,
                        null,
                        sprite.Color,
                        sprite.Rotation,
                        sprite.Origin,
                        sprite.Scale,
                        sprite.SpriteEffects,
                        0f
                    );
                    break;
                case CommandType.BeginPixelate:
                    Main.spriteBatch.Begin(new()
                    {
                        TransformMatrix = Matrix.CreateScale(0.5f),
                    });
                    currentTarget = SwapTarget.HalfScreen;
                    currentTarget.Begin();

                    break;
                case CommandType.EndPixelate:
                    Main.spriteBatch.End();

                    var targetTexture = currentTarget.End();
                    currentTarget = null;

                    Main.spriteBatch.Begin(new()
                    {
                        TransformMatrix = Matrix.CreateScale(2f) * Main.GameViewMatrix.ZoomMatrix,
                    });
                    Main.spriteBatch.Draw(targetTexture, Vector2.Zero, Color.White);
                    Main.spriteBatch.End();
                    break;
                case CommandType.ApplyOutline:
                    var outlineData = _outlineDatas[commands.Datas[i]];
                    targetTexture = currentTarget.Swap();

                    var outlineEffect = Assets.Assets.Effects.Compiled.Pixel.Outline.Value;
                    outlineEffect.Parameters["size"].SetValue(targetTexture.Size());
                    outlineEffect.Parameters["color"].SetValue(outlineData.Color.ToVector4());
                    var snapshot = Main.spriteBatch.CaptureEndBegin(new()
                    {
                        CustomEffect = outlineEffect,
                        TransformMatrix = Matrix.Identity,
                    });
                    Main.spriteBatch.Draw(targetTexture, Vector2.Zero, Color.White);
                    Main.spriteBatch.EndBegin(snapshot);
                    break;
                case CommandType.Begin:
                    Main.spriteBatch.Begin(_beginDatas[commands.Datas[i]]);
                    break;
                case CommandType.End:
                    Main.spriteBatch.End();
                    break;
            }
        }

        if(initialSnapshot is { } s) Main.spriteBatch.Begin(s);
    }

    public readonly struct Pipeline {
        public Pipeline() {
            if(_cache.Count != 0) throw new Exception("One pipeline can be active at a time.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline Begin(SpriteBatchSnapshot snapshot) {
            var index = _beginDatas.Count;
            _beginDatas.Add(snapshot);
            _cache.Add(CommandType.Begin, index);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline End() {
            _cache.Add(CommandType.End, default);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline BeginPixelate() {
            _cache.Add(CommandType.BeginPixelate, default);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline EndPixelate() {
            _cache.Add(CommandType.EndPixelate, default);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Pipeline ApplyOutline(Color color) {
            var index = _outlineDatas.Count;
            _outlineDatas.Add(new() { Color = color });
            _cache.Add(CommandType.ApplyOutline, index);
            return this;
        }

        public readonly Pipeline DrawTrail(
            Vector2[] positions,
            Func<float, float> width,
            Func<float, Color> color,
            Effect effect,
            params ReadOnlySpan<(string, ParameterValue)> parameters
        ) {
            var parameterIndex = _effectParameters.Count;
            var parameterCount = parameters.Length;
            foreach(var (name, value) in parameters) {
                _effectParameters.Add(new()
                {
                    Name = name,
                    Value = value,
                });
            }

            var index = _trailDatas.Count;
            _trailDatas.Add(new()
            {
                Positions = positions,
                Width = width,
                Color = color,
                Effect = effect,
                ParameterIndex = parameterIndex,
                ParameterCount = parameterCount,
            });
            _cache.Add(CommandType.Trail, index);

            return this;
        }

        public readonly Pipeline DrawSprite(
            Texture2D texture,
            Vector2 position,
            Color? color = null, // WHITE
            Rectangle? source = null,
            float rotation = 0f,
            Vector2? origin = null,
            Vector2? scale = null,
            SpriteEffects spriteEffects = SpriteEffects.None
        ) {
            var index = _spriteDatas.Count;
            _spriteDatas.Add(new()
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
            _cache.Add(CommandType.Sprite, index);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Flush() {
            ExecuteCommands(in _cache);
            _cache.Clear();
        }

        public readonly void Schedule(RenderLayer layer) {
            switch(layer) {
                case RenderLayer.BehindTiles:
                    _behindTiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterTiles:
                    _afterTiles.AddRange(in _cache);
                    break;
                case RenderLayer.BehindProjectiles:
                    _behindProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.AfterProjectiles:
                    _afterProjectiles.AddRange(in _cache);
                    break;
                case RenderLayer.BehindNPCs:
                    _behindNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.AfterNPCs:
                    _afterNPCs.AddRange(in _cache);
                    break;
                case RenderLayer.BehindPlayers:
                    _behindPlayers.AddRange(in _cache);
                    break;
                case RenderLayer.AfterPlayers:
                    _afterPlayers.AddRange(in _cache);
                    break;
            }

            _cache.Clear();
        }
    }
}