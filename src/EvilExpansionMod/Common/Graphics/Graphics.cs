using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

internal class Graphics : ILoadable {
    public static GraphicsDevice Device => Main.graphics.GraphicsDevice;

    public static Matrix ScreenTransformMatrix => s_Instance._screenTransformMatrix;
    public static Matrix WorldTransformMatrix => s_Instance._worldTransformMatrix;

    public static Matrix ScreenNDCTransformMatrix => s_Instance._screenNDCTransformMatrix;
    public static Matrix WorldNDCTransformMatrix => s_Instance._worldNDCTransformMatrix;

    public static RenderCommandQueue ImmediateQueue => s_Instance._immediateQueue;

    public static RenderCommandQueue PreDrawTilesQueue => s_Instance._preDrawTilesQueue;
    public static RenderCommandQueue PreDrawNPCsQueue => s_Instance._preDrawNPCsQueue;
    public static RenderCommandQueue PostDrawNPCsQueue => s_Instance._postDrawNPCsQueue;

    private static Graphics s_Instance = null!;

    private Matrix _screenTransformMatrix;
    private Matrix _worldTransformMatrix;

    private Matrix _screenNDCTransformMatrix;
    private Matrix _worldNDCTransformMatrix;

    private readonly RenderCommandQueue _immediateQueue = new(true);

    private readonly RenderCommandQueue _preDrawTilesQueue = new();
    private readonly RenderCommandQueue _preDrawNPCsQueue = new();
    private readonly RenderCommandQueue _postDrawNPCsQueue = new();

    public void Load(Mod mod) {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
        On_Main.DrawCachedProjs += On_Main_DrawCachedProjs;


        s_Instance = this;
    }

    public void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        On_Main.DrawCachedProjs -= On_Main_DrawCachedProjs;

        s_Instance = null!;
    }

    public static RenderPipeline Begin(Matrix? matrix = null) {
        return Begin(2f, matrix);
    }

    public static RenderPipeline BeginPixelated(Matrix? matrix = null) {
        return Begin(0.5f, matrix);
    }

    public static RenderPipeline Begin(float scale, Matrix? matrix = null)
        => new(ImmediateQueue, scale, matrix ?? ScreenTransformMatrix);

    private void PreDrawEverything() {
        _screenTransformMatrix = Main.GameViewMatrix.TransformationMatrix;
        _worldTransformMatrix =
            Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) *
            _screenTransformMatrix;

        var ndcMatrix = Matrix.CreateOrthographicOffCenter(
            0f,
            Main.screenWidth,
            Main.screenHeight,
            0f,
            -1f,
            1f);
        _screenNDCTransformMatrix = _screenTransformMatrix * ndcMatrix;
        _worldNDCTransformMatrix = _worldTransformMatrix * ndcMatrix;

        _immediateQueue.Clear(); // just in case..

        _preDrawTilesQueue.Clear();
        _preDrawNPCsQueue.Clear();
        _postDrawNPCsQueue.Clear();

        PreDrawEverythingRenderer.Instance?.PreDrawEverything();
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            // TODO: Find a better place for thiss...
            PreDrawEverything();
            RenderCommandRunner.Instance.Run(_preDrawTilesQueue);
        }
        else {
            RenderCommandRunner.Instance.Run(_preDrawNPCsQueue);
        }

        orig(self, behindTiles);
        if(!behindTiles) {
            RenderCommandRunner.Instance.Run(_postDrawNPCsQueue);
        }
    }

    private void On_Main_DrawCachedProjs(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) {
        orig(self, projCache, startSpriteBatch);
    }
}
