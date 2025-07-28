using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace EvilExpansionMod.Utilities;

public static class RenderingUtilities {
    public static bool SwitchToRenderTarget(RenderTarget2D renderTarget) {
        GraphicsDevice gD = Main.graphics.GraphicsDevice;

        if (Main.gameMenu || renderTarget is null)
            return false;

        gD.SetRenderTarget(renderTarget);
        gD.Clear(Color.Transparent);
        return true;
    }
}