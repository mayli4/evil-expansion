using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Dusts;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public sealed class PusGlob : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.PusImp.KEY_PusGlob;

    private Vector2[] _trailPositions;

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 180;
        Projectile.alpha = 0;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.2f;
        Projectile.velocity.X *= 0.99f;

        Projectile.rotation += Projectile.velocity.Length() * 0.05f * Projectile.direction;

        if(Projectile.timeLeft < 30) {
            Projectile.alpha = (int)MathHelper.Lerp(0, 255, (30f - Projectile.timeLeft) / 30f);
        }

        _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 7)];
        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = Projectile.Center + Projectile.velocity;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnPusCreep();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Item17, Projectile.position);

        SpawnPusCreep();

        return true;
    }

    private void SpawnPusCreep() {
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Bottom,
            Vector2.Zero,
            ModContent.ProjectileType<PusCreepProjectile>(),
            Projectile.damage / 2,
            0f,
            Main.myPlayer
        );
    }

    public override bool PreDraw(ref Color lightColor) {
        var trailEffect = Assets.Assets.Effects.Trail.CursedSpiritFire.Value;
        Renderer.BeginPipeline(RenderTarget.HalfScreen)
            .DrawTrail(
                _trailPositions,
                static _ => 14f,
                static _ => new Color(98, 90, 40),
                trailEffect,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 34.432f),
                ("mat", MathUtilities.WorldTransformationMatrix),
                ("stepY", 0.25f),
                ("scale", 0.25f),
                ("texture1", Assets.Assets.Textures.Sample.Pebbles.Value),
                ("texture2", Assets.Assets.Textures.Sample.Noise2.Value)
            )
            .DrawSprite(
                Assets.Assets.Textures.Misc.Circle.Value,
                Projectile.Center - Main.screenPosition,
                color: new Color(98, 90, 40),
                origin: 16f * Vector2.One,
                scale: Vector2.One * 0.3f
            )
            .ApplyOutline(new Color(161, 131, 78))
            .Flush();

        return false;
    }
}

public sealed class PusCreepProjectile : ModProjectile, IDrawOverTiles {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.PusImp.KEY_PusGlob;

    private const int lifetime = 165;

    public float Scale => Utils.GetLerpValue(2f, 8f, Math.Abs(Projectile.ai[0]), true);

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.timeLeft = lifetime;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.alpha = 255;
    }

    public override void AI() {
        if(Projectile.timeLeft > lifetime - 15) {
            float fadeInProgress = (lifetime - Projectile.timeLeft) / 15f;
            Projectile.alpha = (int)MathHelper.Lerp(255, 0, fadeInProgress);
        }
        else if(Projectile.timeLeft <= 90) {
            float fadeOutProgress = (90f - Projectile.timeLeft) / 90f;
            Projectile.alpha = (int)MathHelper.Lerp(0, 255, fadeOutProgress);
        }
        else {
            Projectile.alpha = 0;
        }

        if(Main.rand.NextBool(60)) {
            Dust.NewDustPerfect(
                Projectile.position,
                ModContent.DustType<PusGas>(),
                Vector2.Zero,
                100,
                new Color(98, 90, 40)
            );
        }
    }

    public void DrawOverTiles(SpriteBatch spriteBatch) {
        var tex = Assets.Assets.Textures.NPCs.Crimson.PusImp.PusCreepSplat.Value;
        var color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * ((255 - Projectile.alpha) / 255f);
        var scale = new Vector2(1f + Scale * 0.6f, 1f);

        var drawOffsetY = 0f;

        if(Projectile.timeLeft < 90 && Projectile.timeLeft > 30) {
            float extendProgress = Utils.GetLerpValue(90f, 30f, Projectile.timeLeft, true);
            float currentExtendAmount = extendProgress * 0.5f;
            scale.Y += currentExtendAmount;

            drawOffsetY = (tex.Height / 2f) * currentExtendAmount * scale.X;
        }

        var finalDrawOffset = new Vector2(0, 11 + drawOffsetY);

        spriteBatch.Draw(
            tex,
            Projectile.Center - Main.screenPosition + finalDrawOffset,
            null,
            color,
            Projectile.rotation,
            tex.Size() / 2f,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}