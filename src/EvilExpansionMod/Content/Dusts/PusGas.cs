using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Dusts;

internal class PusGas : ModDust {
    public override string Texture => Assets.Assets.Textures.Dusts.KEY_PusGas;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 32 * Main.rand.Next(3), 32, 32);
        dust.fadeIn = Main.rand.Next(120, 250);
        dust.rotation = Main.rand.NextFloatDirection() * 0.25f;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.velocity.Y -= 0.01f;
        dust.velocity *= 0.94f;

        dust.color = Color.Lerp(dust.color, Color.Transparent, 0.005f);
        dust.alpha += 2;
        dust.scale += 0.005f;
        if(dust.alpha >= 255) {
            dust.active = false;
        }
        return false;
    }
}