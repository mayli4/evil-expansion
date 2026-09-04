using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson.Items;

internal class GulpyYoyoChompProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.Items.GulpyYoyo.GulpyYoyoChomp_Top.KEY;

    private const int MaxTimeLeft = 20;

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.hide = false;
        Projectile.timeLeft = MaxTimeLeft;
    }

    public override bool PreDraw(ref Color lightColor) {
        using var pipeline = new RenderPipeline(Renderer.PostDrawNPCsQueue, 1f, Graphics.WorldTransformMatrix);

        var topTexture = TextureAssets.Projectile[Type].Value;
        var bottomTexture = Assets.Images.Crimson.Items.GulpyYoyo.GulpyYoyoChomp_Bottom.Asset.Value;

        var t = (float)Projectile.timeLeft / MaxTimeLeft;

        var moveProgress = t * t * t;
        var scaleProgress = moveProgress * moveProgress;
        var alphaProgress = MathF.Sin(MathHelper.PiOver2 + MathHelper.PiOver2 * (1f - t));

        pipeline.DrawTexture(new()
        {
            Texture = bottomTexture,
            Position = Projectile.Center,
            Rotation = Projectile.rotation,
            Origin = new(
                bottomTexture.Width / 2f,
                bottomTexture.Height / 2f - 10f - moveProgress * 10f),
            Scale = Vector2.One * (1f + scaleProgress),
            Color = lightColor * alphaProgress,
        });

        pipeline.DrawTexture(new()
        {
            Texture = topTexture,
            Position = Projectile.Center,
            Rotation = Projectile.rotation,
            Origin = new(
                bottomTexture.Width / 2f,
                bottomTexture.Height / 2f + 10f + moveProgress * 10f),
            Scale = Vector2.One * (1f + scaleProgress),
            Color = lightColor * alphaProgress,
        });

        return false;
    }
}
