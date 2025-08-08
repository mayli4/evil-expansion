using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption._HellbringerArmor;
public class ShadowOrbProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_ShadowOrb;

    static int MaxTimeLeft = 360;
    ref float FlashAlpha => ref Projectile.ai[0];
    bool Hit { get => Projectile.ai[1] == 1; set => Projectile.ai[1] = value ? 1 : 0; }

    public override void SetDefaults() {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = MaxTimeLeft;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
    }

    public override void AI() {
        Projectile.velocity *= 0.92f;
        if(Projectile.velocity.X != 0f && Projectile.velocity.Y != 0f) {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        FlashAlpha *= 0.95f;

        var owner = Main.player[Projectile.owner];
        if(
            Main.netMode != NetmodeID.MultiplayerClient
            && !Hit
            && owner != null
            && !owner.dead
            && owner.Hitbox.Intersects(Projectile.Hitbox)
        ) {
            Projectile.timeLeft = 45;
            Projectile.velocity += owner.Center.DirectionTo(Projectile.Center) * 10f;
            FlashAlpha = 1f;
            Hit = true;

            Projectile.netUpdate = true;
        }

        if(Projectile.timeLeft == 1 && Hit) {
            var rotation = Main.rand.NextFloat();
            for(int i = 0; i < 3; i++) {
                var direction = rotation.ToRotationVector2();
                Gore.NewGoreDirect(
                    Projectile.GetSource_Death(),
                    Projectile.Center + direction * 10f - new Vector2(8, 8),
                    direction * Main.rand.NextFloat(3f, 5f),
                    Mod.Find<ModGore>("ShadowOrbGore" + i).Type
                );

                rotation += MathF.PI * 2f / 3f + Main.rand.NextFloatDirection() * 0.2f;
            }

            if(Main.netMode != NetmodeID.MultiplayerClient) {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<CorruptlingProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    Main.rand.Next(3)
                );
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var texture = TextureAssets.Projectile[Type].Value;

        var stretchMax = 0.15f;
        var stretch = stretchMax / MathF.Max(Projectile.velocity.Length() * 0.8f, 1f);

        var position = Projectile.Center;
        var scale = Projectile.scale;
        if(Hit) {
            var factor = Math.Max(0f, 1f - (float)Projectile.timeLeft / 45);
            scale *= 1f + 0.2f * factor;
            position += Main.rand.NextVector2Unit() * 1.5f * factor;
        }

        var masterAlpha = 1f;
        if(!Hit && Projectile.timeLeft < 20) {
            masterAlpha = Projectile.timeLeft / 20f;
        }

        var outlineColor = new Color(230, 255, 5);
        var outlineColorBlink = outlineColor
            * MathF.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI * 34.12f)
            * masterAlpha;
        Renderer.BeginPipeline()
            .DrawSprite(
                texture,
                position - Main.screenPosition,
                lightColor * masterAlpha,
                null,
                Projectile.rotation,
                texture.Size() / 2f,
                new Vector2(1f + stretchMax - stretch, 1f - stretchMax + stretch) * scale,
                Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
            )
            .ApplyOutline(outlineColorBlink)
            .ApplyOutline(outlineColorBlink)
            .ApplyTint(outlineColor * masterAlpha * FlashAlpha)
            .Flush();

        return false;
    }
}
