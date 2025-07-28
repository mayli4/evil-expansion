using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption; 

internal class MaceCrack : ModProjectile, IDrawOverTiles {
    public override string Texture => Assets.Assets.KEY_icon;

    public override void SetDefaults()
    {
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 200;
    }

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }

    public void DrawOverTiles(SpriteBatch spriteBatch) {
        var color = Color.White;
        color *= Projectile.timeLeft > 100 ? 1f : Projectile.timeLeft / 100f;
        var tex = Assets.Assets.Textures.Misc.Crack.Value;

        spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
    }
}