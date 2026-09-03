using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal class QuadRenderer : ILoadable {
    public static QuadRenderer Instance { get; private set; } = null!;

    private static Effect QuadEffect => Assets.Shaders.Core.Quad.Asset.Value;
    private DynamicVertexBuffer _quadVertexBuffer = null!;

    public void Load(Mod mod) {
        Main.QueueMainThreadAction(() =>
        {
            _quadVertexBuffer = new DynamicVertexBuffer(Graphics.Device, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
        });
        Instance = this;
    }

    public void Unload() {
        Main.QueueMainThreadAction(() =>
        {
            _quadVertexBuffer.Dispose();
        });
    }

    public void Draw(
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

        Graphics.Device.SetVertexBuffer(_quadVertexBuffer);
        Graphics.Device.Indices = null;

        QuadEffect.Parameters["uMatrix"].SetValue(matrix);
        QuadEffect.CurrentTechnique.Passes[0].Apply();

        if(effect is not null) {
            foreach(var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();

                Graphics.Device.Textures[0] = texture;
                Graphics.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            return;
        }

        Graphics.Device.Textures[0] = texture;
        Graphics.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }
}
