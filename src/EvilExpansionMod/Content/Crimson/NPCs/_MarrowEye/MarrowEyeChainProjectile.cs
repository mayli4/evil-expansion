using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

internal class MarrowEyeChainProjectile : ModProjectile, IPreDrawEverything {
    public override string Texture => Assets.Images.Crimson.NPCs.MarrowEye.MarrowEyeChain.KEY;

    public const int DisapearFrames = 120;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 180;
        Projectile.hide = true;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        Projectile.alpha = 255 - (int)(255f * Projectile.timeLeft / DisapearFrames);
    }

    public void PreDrawEverything() {
        using var pipeline = new RenderPipeline(Graphics.PreDrawTilesQueue, 1f, Graphics.WorldTransformMatrix);

        var texture = TextureAssets.Projectile[Type].Value;
        var alpha = (255 - Projectile.alpha) / 255f;

        var hookSource = new Vector4(0, 0, texture.Width, 14);
        var chainPartSource = new Vector4(0, 16, texture.Width, 44);

        var originOffset = new Vector2(texture.Width / 2f, 42f);
        var direction = Projectile.rotation.ToRotationVector2();

        pipeline.DrawTexture(new()
        {
            Texture = texture,
            Position = Projectile.position,
            Source = hookSource,
            Rotation = Projectile.rotation - MathHelper.PiOver2,
            Color = Lighting.GetColor(Projectile.position.ToTileCoordinates()) * alpha,
            Origin = new Vector2(hookSource.Z, hookSource.W) - originOffset,
        });

        var repeatCount = (int)Projectile.scale / chainPartSource.W;
        for(var i = 0; i < repeatCount; i++) {
            var chainPartPosition = Projectile.position + direction * (i * chainPartSource.W + hookSource.W);
            pipeline.DrawTexture(new()
            {
                Texture = texture,
                Position = chainPartPosition,
                Source = chainPartSource,
                Rotation = Projectile.rotation - MathHelper.PiOver2,
                Color = Lighting.GetColor(chainPartPosition.ToTileCoordinates()) * alpha,
                Origin = new Vector2(chainPartSource.Z, chainPartSource.W) - originOffset,
            });
        }
    }
}
