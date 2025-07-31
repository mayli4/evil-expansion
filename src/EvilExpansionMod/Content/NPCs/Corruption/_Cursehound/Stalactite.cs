using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Corruption;

public sealed class StalactiteProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Corruption.Cursehound.KEY_Stalactites;

    private Rectangle _frame;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 28;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 30;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = false;
    }

    public override void OnSpawn(IEntitySource source) {
        _frame = new Rectangle(18 * Main.rand.Next(3), 0, 16, 28);
    }
    
    public override void AI() {
        Projectile.velocity.Y += 0.3f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }
        
        Projectile.ai[0]++;

        if(Projectile.ai[0] >= 30) {
            Projectile.tileCollide = true;
        }

        if (Projectile.timeLeft < 60) {
            Projectile.alpha = (int)((1 - Projectile.timeLeft / 60f) * 255);
        }
        else {
            Projectile.alpha = 0;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

        for (int i = 0; i < 10; i++) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Corruption, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 0f), 0, default, 1f);
        }
        return true;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            _frame,
            Projectile.GetAlpha(lightColor),
            Projectile.rotation,
            _frame.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );
        
        return false;
    }
}