using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

internal class MaceCrack : ModProjectile, ITileMask {
    public override string Texture => Assets.Assets.KEY_icon;

    private Color _lightColor;

    public override void SetDefaults() {
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 200;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center - Main.screenPosition, CursedSpiritNPC.GhostColor1.ToVector3());
    }

    public override bool PreDraw(ref Color lightColor) {
        _lightColor = lightColor;
        return false;
    }

    public void DrawTileMask(SpriteBatch spriteBatch) {
        var color = _lightColor;
        color *= Projectile.timeLeft > 100 ? 1f : Projectile.timeLeft / 100f;
        var glow = Assets.Assets.Textures.Misc.Glow2.Value;
        var tex = Assets.Assets.Textures.Misc.Crack.Value;
        var tex2 = Assets.Assets.Textures.Misc.CrackBright.Value;

        var glowColor = CursedSpiritNPC.GhostColor1;
        glowColor *= Projectile.timeLeft > 100 ? 1f : Projectile.timeLeft / 100f;

        spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, glowColor * 0.5f, 0, tex.Size() / 2, Projectile.scale - 0.5f, SpriteEffects.None, 0);

        // var data = spriteBatch.CaptureEndBegin(new SpriteBatchSnapshot() with { BlendState = BlendState.Additive });
        // spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition + new Vector2(tex.Height / 2 - 110, tex.Width / 2 - 110), null, glowColor * 0.5f, 0, glow.Size() / 2, new Vector2(3, 3), SpriteEffects.None, 0);
        // spriteBatch.EndBegin(data);
    }
}