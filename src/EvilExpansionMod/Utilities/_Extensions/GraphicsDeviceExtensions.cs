using Microsoft.Xna.Framework.Graphics;

namespace EvilExpansionMod.Utilities;

public static class GraphicsDeviceExtensions {
    public static GraphicsDeviceSnapshot Capture(this GraphicsDevice spriteBatch) {
        return new(spriteBatch);
    }
}