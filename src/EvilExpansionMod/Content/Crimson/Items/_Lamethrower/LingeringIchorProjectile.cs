using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LingeringIchorProjectile : ModProjectile, ITileMask {
    public override string Texture => Helper.PlaceholderTextureKey;
    readonly static int DisappearFrames = 40;
    public override void SetDefaults() {
        Projectile.width = 52;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 5;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Oiled, 1200, false);
        target.AddBuff(BuffID.Ichor, 900, false);
        target.AddBuff(BuffID.OnFire3, 120, false);
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if(Main.rand.NextBool(70)) {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IchorTorch);
        }
    }

    public void DrawTileMask(SpriteBatch spriteBatch) {
        var alpha = MathF.Min(Projectile.timeLeft / (float)DisappearFrames, 1f);
        var texture = Assets.Images.Crimson.Items.Lamethrower.IchorSplat.Asset.Value;
        spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * alpha,
            0f,
            new(texture.Width / 2f, 0f),
            1f,
            SpriteEffects.None,
            0f
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var alpha = MathF.Min(Projectile.timeLeft / (float)DisappearFrames, 1f);

        var snapshot = Main.spriteBatch.CaptureEndBegin(new() { BlendState = BlendState.Additive });
        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.Center - Main.screenPosition,
            null,
            new Color(241, 194, 92) * alpha * 0.05f,
            0f,
            glowTexture.Size() / 2f,
            Main.rand.NextFloat() * 0.1f + 0.9f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(snapshot);
        return false;
    }
}
