using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson._ThoughtfulCultist;
public class CultistPortal : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.ThoughtfulCultist.KEY_CultistBrain;

    public override void SetDefaults() {
        Projectile.width = 60;
        Projectile.height = 120;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 180;

        Projectile.aiStyle = -1;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        var sampleTexture0 = Assets.Assets.Textures.Sample.Pebbles.Value;
        var sampleTexture1 = Assets.Assets.Textures.Sample.Pebbles.Value;

        var effect = Assets.Assets.Effects.Pixel.CultistPortal.Value;
        var destination = new Rectangle(
            (int)(Projectile.position.X - Main.screenPosition.X),
            (int)(Projectile.position.Y - Main.screenPosition.Y),
            Projectile.width,
            Projectile.height
        );

        new Renderer.Pipeline()
            .BeginPixelate(new() { CustomEffect = effect })
            .EffectParams(
                effect,
                ("tex1", sampleTexture1),
                ("time", (50 - Projectile.timeLeft) * 0.5f),
                ("color", Color.Red.ToVector4())
            )
            .DrawSprite(sampleTexture0, destination)
            .ApplyOutline(Color.Yellow)
            .End()
            .Flush();

        return false;
    }
}
