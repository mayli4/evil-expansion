using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class MarrowLazerProjectile : ModProjectile {
    public override string Texture => Assets.Textures.NPCs.Crimson.MarrowEye.MarrowEyeNPC.KEY;
    public static readonly int DisapearFrames = 8;

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = DisapearFrames * 2;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        var hitPoint = Projectile.position + Projectile.velocity * 8f;
        for(var i = 0; i < 200; i++) {
            if(Collision.SolidCollision(hitPoint, 1, 1)) break;

            var foundPlayerCollision = false;
            foreach(var player in Main.player[0..Main.maxPlayers]) {
                if(player.Hitbox.Contains((int)hitPoint.X, (int)hitPoint.Y)) {
                    player.Hurt(new Player.HurtInfo
                    {
                        SoundDisabled = true,
                        DamageSource = PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI),
                        Damage = 1,
                        HitDirection = MathF.Sign(player.Center.X - Projectile.position.X),
                    });

                    foundPlayerCollision = true;
                    break;
                }
            }

            if(foundPlayerCollision) break;
            hitPoint += Projectile.velocity * 8f;
        }

        Projectile.scale = (hitPoint - Projectile.position).Length();
    }

    public override bool PreDraw(ref Color lightColor) {
        var scale = MathF.Sin(MathF.PI * Projectile.timeLeft / (DisapearFrames * 2f));
        var rotation = Projectile.velocity.ToRotation();
        var glowTexture = Assets.Textures.Sample.Glow1.Asset.Value;

        var mainColor = new Color(253, 60, 179);
        var secondaryColor = new Color(63, 28, 72);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position - Main.screenPosition,
            null,
            mainColor * 0.6f,
            rotation,
            glowTexture.Size() / 2f,
            0.2f * scale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position + Projectile.velocity * Projectile.scale / 2f - Main.screenPosition,
            null,
            mainColor * 0.4f,
            rotation,
            glowTexture.Size() / 2f,
            new Vector2(0.6f + Projectile.scale / glowTexture.Width, scale * 0.175f),
            SpriteEffects.None,
            0f
        );
        Main.spriteBatch.EndBegin(snapshot);

        var texture0 = Assets.Textures.Sample.Pebbles.Asset.Value;
        var texture1 = Assets.Textures.Sample.PlasmaNoise.Asset.Value;
        var effect = Assets.Shaders.Pixel.MarrowLaser.Asset.Value;

        Renderer.BeginPipeline(0.5f, Graphics.WorldTransformMatrix)
            .SetEffectParams(
                effect,
                ("uLength", Projectile.scale),
                ("uColor1", secondaryColor.ToVector4()),
                ("uColor2", mainColor.ToVector4()),
                ("uTime", Main.GameUpdateCount * 0.2f)
            )
            .SetTexture(1, texture1)
            .DrawTexture(new()
            {
                Texture = texture0,
                Position = Projectile.position,
                Color = Color.White,
                Rotation = rotation,
                Origin = new Vector2(0, texture0.Height / 2f),
                Scale = new Vector2(Projectile.scale / texture0.Width, scale * 5f / texture0.Height),
                SpriteEffects = SpriteEffects.None,
                Effect = effect,
            })
            .ApplyOutline(mainColor)
            .ApplyOutline(secondaryColor)
            .End();


        return false;
    }
}
