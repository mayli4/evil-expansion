using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public sealed class PusGlob : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.PusImp.KEY_PusGlob;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = 1; 
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 180;
        Projectile.alpha = 0;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.2f;
        Projectile.velocity.X *= 0.99f; 

        Projectile.rotation += Projectile.velocity.Length() * 0.05f * Projectile.direction;

        if (Projectile.timeLeft < 30) {
            Projectile.alpha = (int)MathHelper.Lerp(0, 255, (30f - Projectile.timeLeft) / 30f);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnPusCreep();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Item17, Projectile.position);

        SpawnPusCreep();

        return true;
    }

    private void SpawnPusCreep() {
        // Projectile.NewProjectile(
        //     Projectile.GetSource_FromThis(),
        //     Projectile.Bottom,
        //     Vector2.Zero,
        //     ModContent.ProjectileType<PusCreepProjectile>(),
        //     Projectile.damage / 2,
        //     0f,
        //     Main.myPlayer
        // );
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Vector2 drawOrigin = texture.Size() / 2f;

        Color drawColor = Projectile.GetAlpha(lightColor);

        spriteBatch.Draw(
            texture,
            drawPos,
            null,
            drawColor,
            Projectile.rotation,
            drawOrigin,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );
        return false;
    }
}