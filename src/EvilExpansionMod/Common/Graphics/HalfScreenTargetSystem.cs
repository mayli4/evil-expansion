using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

public class HalfScreenTargetSystem : ModSystem {
    static SwapTarget _target;
    public static SwapTarget Target => _target;

    public override void Load() {
        Main.QueueMainThreadAction(
            () =>
            {
                _target = new(Main.screenWidth / 2, Main.screenHeight / 2);
            }
        );
    }

    public override void Unload() {
        _target.Dispose();
    }

    public override void UpdateUI(GameTime gameTime) {
        if(_target.Width != Main.screenWidth / 2 || _target.Height != Main.screenHeight / 2) {
            Main.QueueMainThreadAction(
                () =>
                {
                    _target.Dispose();
                    _target = new(Main.screenWidth / 2, Main.screenHeight / 2);
                }
            );
        }
    }
}