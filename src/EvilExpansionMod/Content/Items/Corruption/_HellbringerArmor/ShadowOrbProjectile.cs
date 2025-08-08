using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption._HellbringerArmor;
public class ShadowOrbProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_ShadowOrb;

    float _flashAlpha;

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 360;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
    }

    public override void AI() {
        Projectile.velocity *= 0.92f;
        if(Projectile.velocity.X != 0f && Projectile.velocity.Y != 0f) {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        _flashAlpha *= 0.95f;

        var owner = Main.player[Projectile.owner];
        if(owner != null && !owner.dead && owner.Hitbox.Intersects(Projectile.Hitbox)) {
            Projectile.velocity += owner.Center.DirectionTo(Projectile.Center) * 10f;
            _flashAlpha = 1f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var stretchMax = 0.1f;
        var stretch = stretchMax / MathF.Max(MathF.Pow(2f * Projectile.velocity.Length(), 2f), 1f);

        Renderer.BeginPipeline()
            .DrawSprite(
                texture,
                Projectile.Center - Main.screenPosition,
                lightColor,
                null,
                Projectile.rotation,
                texture.Size() / 2f,
                new Vector2(1f + stretchMax - stretch, 1f - stretchMax + stretch),
                SpriteEffects.None
            )
            .ApplyTint(Color.White * _flashAlpha)
            .Flush();

        return false;
    }
}
