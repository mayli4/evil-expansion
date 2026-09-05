using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class MarrowLazerProjectile : ModProjectile {
    public override string Texture => Assets.Images.Crimson.NPCs.MarrowEye.MarrowEyeNPC.KEY;

    public static readonly int DisappearFrames = 16;

    Color mainColor = new(21, 8, 30);
    Color secondaryColor = new(253, 60, 179);
    Color highlightColor = new(255, 215, 240);

    int hitCd;

    float Scale {
        get {
            var scale = 0.025f;
            if(Projectile.timeLeft <= DisappearFrames * 2f) {
                scale += MathF.Sin(MathF.PI * Projectile.timeLeft / (DisappearFrames * 2f)) * 0.975f;
            }

            return scale;
        }
    }

    private SlotId loopSoundSlot = SlotId.Invalid;

    public static readonly SoundStyle LaserLoopSound = new(Assets.Sounds.MarrowEye.MarrowEyeLoopedLaser.KEY)
    {
        IsLooped = true,
        Volume = 0.8f,
        PitchVariance = 0f,
    };

    public override void SetDefaults() {
        Projectile.width = 0;
        Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = false;
        Projectile.timeLeft = DisappearFrames * 2 + 40;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        var hitPoint = Projectile.position + Projectile.velocity * 8f;
        var lightColor = secondaryColor * 0.005f * Scale;

        if(Projectile.timeLeft < DisappearFrames * 2) {
            if(!SoundEngine.TryGetActiveSound(loopSoundSlot, out var activeSound)) {
                if(Projectile.timeLeft > DisappearFrames) {
                    loopSoundSlot = SoundEngine.PlaySound(LaserLoopSound, Projectile.position);
                }
            }
            else {
                activeSound.Position = Projectile.position;

                if(Projectile.timeLeft <= DisappearFrames) {
                    float fadeProgress = (float)Projectile.timeLeft / DisappearFrames;
                    activeSound.Volume = LaserLoopSound.Volume * fadeProgress;
                }
            }
        }

        for(var i = 0; i < 400; i++) {
            if(Collision.SolidCollision(hitPoint, 1, 1)) break;

            var foundPlayerCollision = false;
            foreach(var player in Main.ActivePlayers) {
                if(player.Hitbox.Contains((int)hitPoint.X, (int)hitPoint.Y)) {
                    if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft < DisappearFrames * 2 && hitCd <= 0) {
                        player.Hurt(new Player.HurtInfo
                        {
                            SoundDisabled = true,
                            DamageSource = PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI),
                            Damage = 5,
                            HitDirection = MathF.Sign(player.Center.X - Projectile.position.X),
                        });

                        hitCd = 50;
                    }

                    foundPlayerCollision = true;
                    break;
                }
            }

            Lighting.AddLight(hitPoint, lightColor.R, lightColor.G, lightColor.B);

            if(foundPlayerCollision) break;
            hitPoint += Projectile.velocity * 8f;
        }

        Projectile.scale = (hitPoint - Projectile.position).Length();

        if(Projectile.timeLeft < DisappearFrames * 2 && Main.rand.NextBool(5)) {
            var ember = GlowEmberParticle.NewParticle(
               hitPoint + Main.rand.NextVector2Unit() * 5f,
               Main.rand.NextVector2Unit() * Main.rand.NextFloat(4.2f, 7.5f),
               Main.rand.NextFloat(0.25f, 1.5f),
               secondaryColor,
               highlightColor);

            ember.Randomness *= 2f;
            ember.LossPerSecond *= 2f;
            ParticleEngine.PARTICLES.Add(ember);
        }

        var ember2 = GlowEmberParticle.NewParticle(
           Projectile.Center + Main.rand.NextVector2Unit() * 10f + Projectile.velocity * Projectile.scale * Main.rand.NextFloat(),
           Projectile.velocity * 5f,
           Main.rand.NextFloat(0.5f, 0.75f),
           highlightColor,
           highlightColor);

        ember2.Randomness *= 2f;
        ember2.LossPerSecond *= 2f;
        ember2.Gravity = Vector2.Zero;

        ParticleEngine.PARTICLES.Add(ember2);

        if(hitCd > 0) hitCd--;
    }

    public override void OnKill(int timeLeft) {
        if(SoundEngine.TryGetActiveSound(loopSoundSlot, out var activeSound)) {
            activeSound.Stop();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var rotation = Projectile.velocity.ToRotation();

        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var glowBallTexture = Assets.Images.Sample.GlowBall.Asset.Value;
        var starTexture = Assets.Images.Sample.Star1.Asset.Value;

        var texture0 = Assets.Images.Sample.Trail1.Asset.Value;
        var texture1 = Assets.Images.Sample.PortalNoise.Asset.Value;
        var effect = Assets.Shaders.Pixel.MarrowLaser.Asset.Value;

        Graphics.Begin(Graphics.WorldTransformMatrix)
            .SetEffectParams(
                effect,
                ("uLength", Projectile.scale),
                ("uColor1", highlightColor),
                ("uColor2", secondaryColor),
                ("uColor3", mainColor),
                ("uTime", Main.GameUpdateCount * 0.05f),
                ("uStepThreshold", 0.27f + 0.05f * Scale),
                ("uStepColor1", 0.4f),
                ("uStepColor2", 0.06f),
                ("uPulseSpeed", 5f),
                ("uPulseFreq", 2.67f),
                ("uPulseSize", 0.08f))
            .SetSamplerState(0, SamplerState.LinearWrap)
            .SetTexture(1, texture1, SamplerState.LinearWrap)
            .SetBlendState(BlendState.AlphaBlend)
            .DrawTexture(new()
            {
                Texture = texture0,
                Position = Projectile.position,
                Color = Color.White,
                Rotation = rotation,
                Origin = new Vector2(0, texture0.Height / 2f),
                Scale = new Vector2(
                    Projectile.scale / texture0.Width,
                    Scale * 92f / texture0.Height),
                SpriteEffects = SpriteEffects.None,
                Effect = effect,
            })
            .ApplyOutline(highlightColor)
            .ApplyBloom(1.5f)
            .End();

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position - Main.screenPosition,
            null,
            highlightColor * 0.6f,
            rotation,
            glowTexture.Size() / 2f,
            Scale * 0.1f + Main.rand.NextFloat() * 0.05f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            starTexture,
            Projectile.position - Main.screenPosition,
            null,
            highlightColor,
            0.32f + Main.rand.NextFloat() * 0.1f,
            starTexture.Size() / 2f,
            Scale * 0.2f + Main.rand.NextFloat() * 0.25f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowBallTexture,
            Projectile.position - Main.screenPosition,
            null,
            highlightColor * 0.25f,
            0.32f + Main.rand.NextFloat() * 0.1f,
            glowBallTexture.Size() / 2f,
            Scale * 0.3f + Main.rand.NextFloat() * 0.075f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position + Projectile.velocity * Projectile.scale / 2f - Main.screenPosition,
            null,
            highlightColor * 0.4f,
            rotation,
            glowTexture.Size() / 2f,
            new Vector2(0.6f + Projectile.scale / glowTexture.Width, Scale * 0.175f),
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position + Projectile.velocity * Projectile.scale - Main.screenPosition,
            null,
            highlightColor * 0.6f,
            rotation,
            glowTexture.Size() / 2f,
            Scale * 0.1f + Main.rand.NextFloat() * 0.05f,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.EndBegin(ss);

        return false;
    }
}
