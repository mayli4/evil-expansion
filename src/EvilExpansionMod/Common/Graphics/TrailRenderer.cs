using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal class TrailRenderer : ILoadable {
    public static TrailRenderer Instance { get; private set; } = null!;

    public delegate Color ColorFunc(float t);
    public delegate float WidthFunction(float t);

    private const int TrailPositionCapacity = 256;
    private const int TrailVertexCount = TrailPositionCapacity * 2;
    private const int TrailIndexCount = (TrailPositionCapacity - 1) * 6;

    private static Effect TrailEffect => Assets.Shaders.Core.Trail.Asset.Value;

    private DynamicVertexBuffer _trailVertexBuffer = null!;
    private readonly VertexPositionColorTexture[] _trailVertices = new VertexPositionColorTexture[TrailVertexCount];

    private DynamicIndexBuffer _trailIndexBuffer = null!;
    private readonly ushort[] _trailIndices = new ushort[TrailIndexCount];

    public void Load(Mod mod) {
        Main.QueueMainThreadAction(() =>
        {
            _trailVertexBuffer = new(
                Graphics.Device,
                typeof(VertexPositionColorTexture),
                TrailPositionCapacity * 2,
                BufferUsage.WriteOnly);

            _trailIndexBuffer = new(
                Graphics.Device,
                IndexElementSize.SixteenBits,
                (TrailPositionCapacity - 1) * 6,
                BufferUsage.WriteOnly);
        });

        Instance = this;
    }

    public void Unload() {
        Main.QueueMainThreadAction(() =>
        {
            _trailVertexBuffer.Dispose();
            _trailIndexBuffer.Dispose();
        });
    }

    public void Draw(
        ReadOnlySpan<Vector2> positions,
        WidthFunction widthFn,
        ColorFunc colorFn,
        Effect effect) {
        PrepareVerticesAndIndices(positions, widthFn, colorFn);

        foreach(var pass in effect.CurrentTechnique.Passes) {
            pass.Apply();

            Graphics.Device.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                positions.Length * 2,
                0,
                (positions.Length - 1) * 2
            );
        }
    }

    public void Draw(
        ReadOnlySpan<Vector2> positions,
        WidthFunction widthFn,
        ColorFunc colorFn,
        Matrix matrix,
        int spriteRotation,
        Effect? effect = null) {
        PrepareVerticesAndIndices(positions, widthFn, colorFn);

        TrailEffect.Parameters["uMatrix"].SetValue(matrix);
        TrailEffect.Parameters["uSpriteRotation"].SetValue(spriteRotation);
        TrailEffect.CurrentTechnique.Passes[0].Apply();

        if(effect is not null) {
            foreach(var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();

                Graphics.Device.DrawIndexedPrimitives(
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

        Graphics.Device.DrawIndexedPrimitives(
            PrimitiveType.TriangleList,
            0,
            0,
            positions.Length * 2,
            0,
            (positions.Length - 1) * 2
        );
    }

    private void PrepareVerticesAndIndices(
        ReadOnlySpan<Vector2> positions,
        WidthFunction widthFn,
        ColorFunc colorFn) {
        var color = colorFn(0f);

        var vertexOffset = positions[0]
            .DirectionTo(positions[1])
            .RotatedBy(MathHelper.PiOver2) * widthFn(0f) * 0.5f;

        _trailVertices[0] = new VertexPositionColorTexture((positions[0] - vertexOffset).ToVector3(), color, Vector2.Zero);
        _trailVertices[1] = new VertexPositionColorTexture((positions[0] + vertexOffset).ToVector3(), color, Vector2.UnitY);

        for(var j = 1; j < positions.Length; j++) {
            var factor = j / (positions.Length - 1f);

            color = colorFn(factor);

            var currentPosition = positions[j];
            var previousPosition = positions[j - 1];

            vertexOffset =
                previousPosition.DirectionTo(currentPosition).RotatedBy(MathHelper.PiOver2) * widthFn(factor) * 0.5f;

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
        Graphics.Device.SetVertexBuffer(_trailVertexBuffer);

        _trailIndexBuffer.SetData(_trailIndices);
        Graphics.Device.Indices = _trailIndexBuffer;
    }
}
