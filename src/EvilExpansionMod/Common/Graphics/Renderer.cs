using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

// TODO: for now leaving this here until i figure out how to do this better
internal class Renderer : ILoadable {
    public static RenderCommandQueue PostDrawNPCsQueue { get; } = new();

    private static readonly RenderCommandQueue s_ImmediateQueue = new(true);

    public void Load(Mod mod) {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }

    public void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        orig(self, behindTiles);
        if(!behindTiles) RenderCommandRunner.Instance.Run(PostDrawNPCsQueue);
    }

    public static RenderPipeline Begin(Matrix? matrix = null) {
        return Begin(2f, matrix);
    }

    public static RenderPipeline BeginPixelated(Matrix? matrix = null) {
        return Begin(0.5f, matrix);
    }

    public static RenderPipeline Begin(float scale, Matrix? matrix = null)
        => new(s_ImmediateQueue, scale, matrix ?? Graphics.ScreenTransformMatrix);
}
