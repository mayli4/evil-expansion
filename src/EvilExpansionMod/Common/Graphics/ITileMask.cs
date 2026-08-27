using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Daybreak.Common.Rendering;
using EvilExpansionMod.Core;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

public interface ITileMask {
    void DrawTileMask(SpriteBatch spriteBatch);
}

file static class Impl {
    internal sealed class Buffers : IStatic<Buffers> {
        public required RenderTargetLease MaskTargetLease { get; init; }
        public required RenderTargetLease SolidTilesTargetLease { get; init; }
        
        public static Buffers LoadData(Mod mod) {
            return Main.RunOnMainThread(() => new Buffers
            {
                MaskTargetLease = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice),
                SolidTilesTargetLease = ScreenspaceTargetPool.Shared.Rent(Main.graphics.GraphicsDevice),
            }).GetAwaiter().GetResult();
        }
        
        public static void UnloadData(Buffers data) {
            Main.RunOnMainThread(() => {
                data.MaskTargetLease.Dispose();
                data.SolidTilesTargetLease.Dispose();
            });
        }
    }
    
    private static readonly List<ITileMask> render_queue = new();

    [OnLoad, UsedImplicitly]
    public static void Load() {
        On_Main.DoDraw_Tiles_Solid += HookPostDrawTilesSolid;
        On_Main.DrawProjectiles += DrawSolidMask;
    }

    [OnUnload, UsedImplicitly]
    public static void Unload() {
        On_Main.DoDraw_Tiles_Solid -= HookPostDrawTilesSolid;
        On_Main.DrawProjectiles -= DrawSolidMask;
    }

    private static void HookPostDrawTilesSolid(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
        orig(self);
    
        render_queue.Clear();
        foreach(var proj in Main.ActiveProjectiles) {
            if (proj.active && proj.ModProjectile is ITileMask maskDraw) {
                render_queue.Add(maskDraw);
            }
        }

        var data = IStatic<Buffers>.Instance;

        var sb = Main.spriteBatch;

        using (data.SolidTilesTargetLease.Scope(preserveContents: false, clearColor: Color.Transparent)) {
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone
            );
            
            sb.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
            sb.End();
        }

        using (data.MaskTargetLease.Scope(preserveContents: false, clearColor: Color.Transparent)) {
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone
            );

            foreach(var t in render_queue) {
                t.DrawTileMask(Main.spriteBatch);
            }

            sb.End();
        }

        render_queue.Clear();
    }

    private static void DrawSolidMask(On_Main.orig_DrawProjectiles orig, Main self) {
        var shader = Assets.Shaders.Pixel.TileMask.CreatePixelPass();
        var data = IStatic<Buffers>.Instance;

        shader.Parameters.MaskSampler = new HlslSampler()
        {
            Texture = data.SolidTilesTargetLease.Target,
            Sampler = SamplerState.PointClamp
        };
        
        Main.spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.Default,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        shader.Apply();

        Main.spriteBatch.Draw(data.MaskTargetLease.Target, Vector2.Zero, Color.White);
        Main.spriteBatch.End();

        orig(self);
    }
}