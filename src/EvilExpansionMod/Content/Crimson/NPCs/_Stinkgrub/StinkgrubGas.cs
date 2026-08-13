using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class StinkgrubGasProjectile : ModProjectile {
    public override string Texture => Assets.Textures.NPCs.Crimson.Stinkgrub.PusBottle.KEY;

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}