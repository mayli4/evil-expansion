using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

public class HalfScreenTargetSystem : ModSystem {
    public static SwapTarget Target { get; private set; }

    public override void Load() {
        Main.QueueMainThreadAction(() =>
        {
            Target = new(Main.screenWidth / 2, Main.screenHeight / 2);
        });
    }

    public override void Unload() {
        Main.QueueMainThreadAction(() =>
        {
            Target.Dispose();
        });
    }

    public override void UpdateUI(GameTime gameTime) {
        if(Target.Width != Main.screenWidth / 2 || Target.Height != Main.screenHeight / 2) {
            Main.QueueMainThreadAction(() =>
            {
                Target.Dispose();
                Target = new(Main.screenWidth / 2, Main.screenHeight / 2);
            });
        }
    }
}