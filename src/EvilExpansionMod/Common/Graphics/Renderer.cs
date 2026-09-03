using Microsoft.Xna.Framework;

namespace EvilExpansionMod.Common.Graphics;

// TODO: for now leaving this here until i figure out how to do this better
internal class Renderer {
    private static readonly RenderCommandQueue s_Queue = new();

    public static RenderPipeline Begin(Matrix? matrix = null) {
        return Begin(2f, matrix);
    }

    public static RenderPipeline BeginPixelated(Matrix? matrix = null) {
        return Begin(0.5f, matrix);
    }

    public static RenderPipeline Begin(float scale, Matrix? matrix = null)
        => new(s_Queue, scale, matrix ?? Graphics.ScreenTransformMatrix);
}
