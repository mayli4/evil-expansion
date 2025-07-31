using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

public interface IDrawOverTiles {
    void DrawOverTiles(SpriteBatch spriteBatch);
}

internal sealed class TileMasking : ModSystem {
    public static RenderTarget2D MaskTarget;
    public static RenderTarget2D SolidTilesTarget;
    public static readonly List<IDrawOverTiles> RenderQueue = [];

    private static bool _refreshTarget;

    private Vector2 _oldScreenSize;

    public override void Load() {
        On_Main.CheckMonoliths += DrawToRenderTargets;
        On_Main.DrawProjectiles += DrawSolidMask;
    }

    public override void PostUpdateEverything() {
        CheckScreenSize();
    }

    private void CheckScreenSize() {
        if(Main.dedServ || Main.gameMenu) {
            return;
        }

        Vector2 newScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        if(_oldScreenSize != newScreenSize) {
            ResizeRenderTargets();
        }
        _oldScreenSize = newScreenSize;
    }

    private void ResizeRenderTargets() {
        if(Main.dedServ) {
            return;
        }

        Main.QueueMainThreadAction(() =>
        {
            DisposeTargets();
            MaskTarget = new RenderTarget2D(
                Main.graphics.GraphicsDevice,
                Main.screenWidth,
                Main.screenHeight
            );
            SolidTilesTarget = new RenderTarget2D(
                Main.graphics.GraphicsDevice,
                Main.screenWidth,
                Main.screenHeight
            );
        });
    }

    public void DisposeTargets() {
        if(MaskTarget != null && !MaskTarget.IsDisposed) {
            MaskTarget.Dispose();
        }
        if(SolidTilesTarget != null && !SolidTilesTarget.IsDisposed) {
            SolidTilesTarget.Dispose();
        }
    }

    public override void Unload() {
        if(Main.dedServ) {
            return;
        }

        Main.QueueMainThreadAction(() =>
        {
            DisposeTargets();
        });

        On_Main.CheckMonoliths -= DrawToRenderTargets;
        On_Main.DrawProjectiles -= DrawSolidMask;
    }

    private void DrawToRenderTargets(On_Main.orig_CheckMonoliths orig) {
        orig();

        if(Main.dedServ || Main.spriteBatch == null || Main.graphics.GraphicsDevice == null) {
            return;
        }

        RenderQueue.Clear();

        for(int i = 0; i < Main.maxProjectiles; i++) {
            Projectile proj = Main.projectile[i];
            if(proj.active && proj.ModProjectile is IDrawOverTiles maskDraw) {
                RenderQueue.Add(maskDraw);
            }
        }

        _refreshTarget = RenderQueue.Count > 0;

        if(!_refreshTarget) {
            return;
        }

        if(SolidTilesTarget == null || SolidTilesTarget.IsDisposed || SolidTilesTarget.IsContentLost) {
            ResizeRenderTargets();
            if(SolidTilesTarget == null || SolidTilesTarget.IsDisposed || SolidTilesTarget.IsContentLost) return;
        }
        if(MaskTarget == null || MaskTarget.IsDisposed || MaskTarget.IsContentLost) {
            ResizeRenderTargets();
            if(MaskTarget == null || MaskTarget.IsDisposed || MaskTarget.IsContentLost) return;
        }

        RenderingUtilities.SwitchToRenderTarget(SolidTilesTarget);
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone,
            null
        );
        Main.spriteBatch.Draw(
            Main.instance.tileTarget,
            Main.sceneTilePos - Main.screenPosition,
            Color.White
        );
        Main.spriteBatch.End();

        RenderingUtilities.SwitchToRenderTarget(MaskTarget);
        Main.spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone
        );

        for(int i = 0; i < RenderQueue.Count; i++) {
            RenderQueue[i].DrawOverTiles(Main.spriteBatch);
        }

        Main.spriteBatch.End();
        RenderQueue.Clear();
    }

    private void DrawSolidMask(On_Main.orig_DrawProjectiles orig, Main self) {
        orig(self);

        if(!_refreshTarget || MaskTarget == null || SolidTilesTarget == null) {
            return;
        }

        Effect effect = Assets.Assets.Effects.Pixel.TileMask.Value;

        if(effect is null) {
            return;
        }

        effect.Parameters["mask"].SetValue(SolidTilesTarget);

        Main.spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone,
            effect,
            Main.GameViewMatrix.TransformationMatrix
        );

        Main.spriteBatch.Draw(MaskTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        Main.spriteBatch.End();
    }
}