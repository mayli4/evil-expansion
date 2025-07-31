using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;
public class CultistPortal : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_CultistBrain;

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 60;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 50;

        Projectile.aiStyle = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        var sampleTexture0 = Assets.Assets.Textures.Sample.BubblyNoise.Value;
        var sampleTexture1 = Assets.Assets.Textures.Sample.Flame1.Value;

        var effect = Assets.Assets.Effects.Compiled.Pixel.CultistPortal.Value;
        var destination = new Rectangle(
            (int)(Projectile.position.X - Main.screenPosition.X),
            (int)(Projectile.position.Y - Main.screenPosition.Y),
            Projectile.width,
            Projectile.height
        );

        new Renderer.Pipeline()
            .EffectParams(
                effect,
                ("tex1", sampleTexture1),
                ("time", (50 - Projectile.timeLeft) * 0.1f),
                ("color", Color.White.ToVector4())
            )
            .BeginPixelate(new() { CustomEffect = effect })
            .DrawSprite(sampleTexture0, destination)
            .End()
            .Flush();

        return false;
    }
}
