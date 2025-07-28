using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace EvilExpansionMod.Utilities;

public static class RenderingUtilities {
    public static bool SwitchToRenderTarget(RenderTarget2D renderTarget) {
        GraphicsDevice gD = Main.graphics.GraphicsDevice;

        if(Main.gameMenu || renderTarget is null)
            return false;

        gD.SetRenderTarget(renderTarget);
        gD.Clear(Color.Transparent);
        return true;
    }

    public static void DrawVFX(Action action, Color? outlineColor = null) {
        var target = SwapTarget.HalfScreen;
        target.Begin();

        action();

        var targetTexture = target.Swap();
        var outlineEffect = Assets.Assets.Effects.Compiled.Pixel.Outline.Value;
        outlineEffect.Parameters["size"].SetValue(targetTexture.Size());
        outlineEffect.Parameters["color"].SetValue((outlineColor ?? Color.White).ToVector3());
        Main.spriteBatch.Begin(new()
        {
            CustomEffect = outlineEffect,
        });
        Main.spriteBatch.Draw(targetTexture, Vector2.Zero, Color.White);
        Main.spriteBatch.End();

        targetTexture = target.End();
        Main.spriteBatch.Draw(targetTexture, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);
    }
}