using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
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

namespace EvilExpansionMod.Content.Crimson;

public sealed class PusGlob : ModProjectile {
    public override string Texture => Assets.Images.Crimson.NPCs.PusImp.PusGlob.KEY;

    private Vector2[] _trailPositions;
    private bool hasCollided = false;
    public ref float SpawnedByGrub => ref Projectile.ai[1];

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = 1180;
        Projectile.alpha = 0;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.2f;
        Projectile.velocity.X *= 0.99f;

        Projectile.rotation += Projectile.velocity.Length() * 0.05f * Projectile.direction;

        if(Projectile.timeLeft < 30) {
            Projectile.alpha = (int)MathHelper.Lerp(0, 255, (30f - Projectile.timeLeft) / 30f);
        }

        _trailPositions ??= [.. Enumerable.Repeat(Projectile.Center, 17)];
        var i = _trailPositions.Length - 1;
        while(i > 0) {
            _trailPositions[i] = _trailPositions[i - 1];
            i -= 1;
        }
        _trailPositions[0] = Projectile.Center + Projectile.velocity;

        if(Projectile.lavaWet && Projectile.timeLeft > 4) {
            // Soo... timeleft = 0 leads to fizzle, higher values lead to splat (ref below)
            Projectile.timeLeft = 4;
        }
    }
    public override void OnKill(int timeLeft) {
        if(Projectile.lavaWet) {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.LiquidsWaterLava with { PitchVariance = 0.5f }, Projectile.position);
            for(int i = 0; i < 4; i++) {
                var dustPos = Projectile.position + Main.rand.NextVector2Circular(10f, 60f) + new Vector2(0f, -5f);
                var dustVelocity = -Vector2.UnitY * Main.rand.NextFloat(30f, 600f)
                    + Projectile.position.DirectionTo(dustPos) * 5f;

                var dustColorStart = new Color(133, 122, 94);
                var dustColorFade = dustColorStart * 0.4f;

                var newDustData = new Smoke.Data()
                {
                    InitialLifetime = 40,
                    ElapsedFrames = 0,
                    InitialOpacity = 0.8f,
                    ColorStart = dustColorStart,
                    ColorFade = dustColorFade,
                    Spin = 0.03f,
                    InitialScale = Main.rand.NextFloat(0.3f, 1.0f)
                };

                var newDust = Dust.NewDustPerfect(
                    dustPos,
                    ModContent.DustType<Smoke>(),
                    dustVelocity,
                    0,
                    newColor: Color.White,
                    newDustData.InitialScale
                );

                newDust.customData = newDustData;
                int dustIndex = Dust.NewDust(
                    dustPos,
                    Projectile.width,
                    Projectile.height,
                    DustID.Smoke,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-7f, 0f),
                    (int)Main.rand.NextFloat(0f, 100f),
                    default,
                    Main.rand.NextFloat(0.5f, 1.5f)
                );
            }
        }
        else { 
            Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.4f, Pitch = Main.rand.NextFloat(-0.8f, 0.1f) }, Projectile.position);
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        SpawnPusCreep();
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Item17, Projectile.position);

        SpawnPusCreep();

        if(SpawnedByGrub == 1f) {
            NPC.NewNPC(
                Projectile.GetSource_FromThis(),
                (int)Projectile.Center.X + Main.rand.Next(-10, 10),
                (int)Projectile.Center.Y + Main.rand.Next(-10, 10),
                ModContent.NPCType<PusImpNPC>()
            );
        }

        if(!hasCollided) {
            hasCollided = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 7;
        }
        return false;
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
        var trailEffect = Assets.Shaders.Trail.CursedSpiritFire.Asset.Value;

        var color = new Color(98, 90, 40).MultiplyRGB(lightColor);
        var outlineColor = new Color(161, 131, 78).MultiplyRGB(lightColor);

        Graphics.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetEffectParams(
                trailEffect,
                ("time", 0.025f * Main.GameUpdateCount + Projectile.whoAmI * 34.432f),
                ("mat", Graphics.WorldTransformMatrix),
                ("stepY", 0.15f),
                ("scale", 0.5f),
                ("texture1", Assets.Images.Sample.Pebbles.Asset.Value),
                ("texture2", Assets.Images.Sample.Pebbles.Asset.Value)
            )
            .DrawTrail(
                _trailPositions,
                static _ => 20f,
                _ => color,
                trailEffect
            )
            .DrawTexture(new()
            {
                Texture = Assets.Images.Misc.Circle.Asset.Value,
                Position = Projectile.Center,
                Color = color,
                Origin = 16f * Vector2.One,
                Scale = Vector2.One * 0.5f,
            })
            .ApplyOutline(outlineColor)
            .End();

        return false;
    }
}

public sealed class PusCreepProjectile : ModProjectile, ITileMask {
    public override string Texture => Assets.Images.Crimson.NPCs.PusImp.PusGlob.KEY;

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
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
        // No KB?
        modifiers.Knockback *= 0.5f;
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

        if(Main.rand.NextBool(4)) {
            var particle = SmokeParticle.Pool.RequestParticle();

            Vector2 randomVelocity = new Vector2(
                Main.rand.NextFloat(-1.5f, 1.5f),
                Main.rand.NextFloat(-2f, -0.5f)
            );

            Color smokeColor = Color.Lerp(Color.White, Color.Black, Main.rand.NextFloat());
            float scale = Main.rand.NextFloat(0.1f, 1.2f);
            int lifetime = Main.rand.Next(20, 60);

            particle.Spawn(Projectile.Center + Main.rand.NextVector2Circular(15, 45), randomVelocity, smokeColor, scale, lifetime);
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public void DrawTileMask(SpriteBatch spriteBatch) {
        var tex = Assets.Images.Crimson.NPCs.PusImp.PusCreepSplat.Asset.Value;
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