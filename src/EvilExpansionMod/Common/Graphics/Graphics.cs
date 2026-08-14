using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

internal class Graphics : ILoadable {
    static Graphics s_Instance = null!;

    private Matrix _worldTransformMatrix;
    public static Matrix WorldTransformMatrix => s_Instance._worldTransformMatrix;

    private Matrix _screenTransformMatrix;
    public static Matrix ScreenTransformMatrix => s_Instance._screenTransformMatrix;

    public void Load(Mod mod) {
        s_Instance = this;
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }

    public void Unload() { }

    void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        if(behindTiles) {
            _screenTransformMatrix = Main.GameViewMatrix.TransformationMatrix *
                Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            _worldTransformMatrix = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) *
                _screenTransformMatrix;
        }

        orig(self, behindTiles);
    }
}
