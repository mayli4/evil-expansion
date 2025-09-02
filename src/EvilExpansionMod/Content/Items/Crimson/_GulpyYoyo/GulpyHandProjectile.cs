using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class GulpyHandProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.GulpyYoyo.KEY_GulpyYoyoGrab;
    bool Open { get => Projectile.ai[1] == 0; set => Projectile.ai[1] = value ? 1f : 0f; }
    int _timer;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = 99;
        Projectile.ownerHitCheck = true;
        Projectile.localNPCHitCooldown = 999;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void AI() {
        var headProjectile = Main.projectile[(int)Projectile.ai[0]];
        if(headProjectile.active) Projectile.timeLeft = 2;

        Projectile.rotation = (Projectile.Center - headProjectile.Center).ToRotation() + MathHelper.PiOver2;

        if(Open) {
            if(!Projectile.MinionTryGetTarget(800, false, true, out var target)) {
                Open = false;
                Projectile.netUpdate = true;
                return;
            }

            var delta = target.Center - Projectile.Center;
            var distance = delta.Length();
            if(distance <= Math.Min(target.width, target.height) + 0.2f) {
                var force = target.Center.DirectionTo(headProjectile.Center) * 2f;
                target.velocity += force;
                Projectile.velocity += force;

                Open = false;
                return;
            }

            Projectile.velocity += 1.1f * delta / distance;
            Projectile.velocity *= 0.9f;
        }
        else {
            Projectile.Center = Vector2.Lerp(Projectile.Center, headProjectile.Center, 0.1f);
            Projectile.velocity *= 0.95f;

            if(_timer > 120) {
                Open = true;
                _timer = 0;
            }
            else _timer += 1;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var headProjectile = Main.projectile[(int)Projectile.ai[0]];

        var texture = TextureAssets.Projectile[Type].Value;
        var source = new Rectangle(
            0,
            (Open ? 0 : 1) * texture.Height / 2,
            texture.Width,
            texture.Height / 2
        );

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            source,
            lightColor,
            Projectile.rotation,
            texture.Size() / 2f,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}
