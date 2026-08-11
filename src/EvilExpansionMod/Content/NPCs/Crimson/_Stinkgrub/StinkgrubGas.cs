using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class StinkgrubGasProjectile : ModProjectile {
    public override string Texture => Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_PusBottle;

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}