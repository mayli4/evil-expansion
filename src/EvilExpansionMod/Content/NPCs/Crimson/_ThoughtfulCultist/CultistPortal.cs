using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;
public class CultistPortal : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_PortalSpear;

    static int MaxTimeLeft = 120;
    public override void SetDefaults() {
        Projectile.width = 45;
        Projectile.height = 140;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxTimeLeft;

        Projectile.aiStyle = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        var t = (float)Projectile.timeLeft / MaxTimeLeft;
        if(t < 0.85f || t > 0.9f) return false;

        float _ = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            Projectile.Center + Projectile.velocity * 110f,
            10,
            ref _
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var spearTexture = TextureAssets.Projectile[Type].Value;
        var sampleTexture0 = Assets.Assets.Textures.Sample.BubblyNoise.Value;
        var sampleTexture1 = Assets.Assets.Textures.Sample.DissolveNoise.Value;

        var effect = Assets.Assets.Effects.Pixel.CultistPortal.Value;
        var destination = new Rectangle(
            (int)(Projectile.Center.X - Main.screenPosition.X),
            (int)(Projectile.Center.Y - Main.screenPosition.Y),
            Projectile.width,
            Projectile.height
        );

        var t = (float)Projectile.timeLeft / MaxTimeLeft;
        var scale = t < 0.1f ? t / 0.1f : (t > 0.9f ? (0.1f - (t - 0.9f)) / 0.1f : 1f);

        var color1 = new Color(209, 54, 0);
        var color2 = new Color(249, 197, 55);
        var rotation = Projectile.velocity.ToRotation();
        new Renderer.Pipeline()
            .BeginPixelate(new() { CustomEffect = effect })
            .EffectParams(
                effect,
                ("tex1", sampleTexture1),
                ("size", scale),
                ("time", Main.GameUpdateCount * 0.05f),
                ("color1", color1.ToVector4()),
                ("color2", color2.ToVector4())
            )
            .DrawSprite(
                sampleTexture0,
                destination,
                rotation: rotation,
                origin: new(Projectile.width, Projectile.height * 2)
            )
            .ApplyOutline(color2)
            .ApplyOutline(Color.DarkBlue)
            .End()
            .Flush();

        Main.spriteBatch.Draw(
            spearTexture,
            Projectile.Center - Main.screenPosition + Projectile.velocity * 12f,
            new Rectangle(0, 0, (int)(scale * spearTexture.Width), spearTexture.Height),
            lightColor,
            rotation,
            Vector2.UnitY * 18,
            1f,
            SpriteEffects.FlipHorizontally,
            0f
        );
        return false;
    }
}
