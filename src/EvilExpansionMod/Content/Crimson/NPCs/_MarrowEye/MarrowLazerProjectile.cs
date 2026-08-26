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

    public static readonly int DisapearFrames = 16;

    Color _mainColor = new(63, 28, 72);
    Color _secondaryColor = new(253, 60, 179);
    Color _highlightColor = new(255, 155, 220);

    int _hitCd;

    float Scale {
        get {
            var scale = 0.2f;
            if(Projectile.timeLeft <= DisapearFrames * 2f) {
                scale += MathF.Sin(MathF.PI * Projectile.timeLeft / (DisapearFrames * 2f)) * 0.8f;
            }

            return scale;
        }
    }
    
    private SlotId loopSoundSlot = SlotId.Invalid;

    public static readonly SoundStyle LaserLoopSound = new(Assets.Sounds.MarrowEye.MarrowEyeLoopedLaser.KEY) {
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
        Projectile.timeLeft = DisapearFrames * 2 + 40;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        var hitPoint = Projectile.position + Projectile.velocity * 8f;
        var lightColor = _secondaryColor * 0.005f * Scale;
        
        if (Projectile.timeLeft < DisapearFrames * 2) {
            if (!SoundEngine.TryGetActiveSound(loopSoundSlot, out var activeSound)) {
                if (Projectile.timeLeft > DisapearFrames) {
                    loopSoundSlot = SoundEngine.PlaySound(LaserLoopSound, Projectile.position);
                }
            } else {
                activeSound.Position = Projectile.position;

                if (Projectile.timeLeft <= DisapearFrames) {
                    float fadeProgress = (float)Projectile.timeLeft / DisapearFrames;
                    activeSound.Volume = LaserLoopSound.Volume * fadeProgress;
                }
            }
        }

        for(var i = 0; i < 400; i++) {
            if(Collision.SolidCollision(hitPoint, 1, 1)) break;

            var foundPlayerCollision = false;
            foreach(var player in Main.player[0..Main.maxPlayers]) {
                if(player.Hitbox.Contains((int)hitPoint.X, (int)hitPoint.Y)) {
                    if(Main.netMode != NetmodeID.MultiplayerClient && Projectile.timeLeft < DisapearFrames * 2 && _hitCd <= 0) {
                        player.Hurt(new Player.HurtInfo
                        {
                            SoundDisabled = true,
                            DamageSource = PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI),
                            Damage = 5,
                            HitDirection = MathF.Sign(player.Center.X - Projectile.position.X),
                        });

                        _hitCd = 50;
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

        if (Projectile.timeLeft < DisapearFrames * 2 && Main.rand.NextBool(5)) {
             var ember = GlowEmberParticle.NewParticle(
                hitPoint + Main.rand.NextVector2Unit() * 5f,
                Main.rand.NextVector2Unit() * Main.rand.NextFloat(4.2f, 7.5f),
                Main.rand.NextFloat(0.25f, 1.5f),
                _secondaryColor,
                _highlightColor);

            ember.Randomness *= 2f;
            ember.LossPerSecond *= 2f;
            ParticleEngine.PARTICLES.Add(ember);
        }

        if (_hitCd > 0) _hitCd--;
    }
    
    public override void OnKill(int timeLeft) {
        if (SoundEngine.TryGetActiveSound(loopSoundSlot, out var activeSound)) {
            activeSound.Stop();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        var rotation = Projectile.velocity.ToRotation();

        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var glowBallTexture = Assets.Images.Sample.GlowBall.Asset.Value;
        var starTexture = Assets.Images.Sample.Star1.Asset.Value;

        var texture0 = Assets.Images.Sample.DissolveNoise.Asset.Value;
        var texture1 = Assets.Images.Sample.Noise1.Asset.Value;
        var effect = Assets.Shaders.Pixel.MarrowLaser.Asset.Value;

        Renderer.BeginPixelated(Graphics.WorldTransformMatrix)
            .SetEffectParams(
                effect,
                ("uLength", Projectile.scale),
                ("uColor1", _secondaryColor),
                ("uColor2", _mainColor),
                ("uColor3", _highlightColor),
                ("uTime", -Main.GameUpdateCount * 0.008f),
                ("uStepThreshold", 0.02f + 0.05f * Scale),
                ("uStepColor", 0.16f)
            )
            .SetTexture(1, texture1)
            .SetBlendState(BlendState.AlphaBlend)
            .DrawTexture(new()
            {
                Texture = texture0,
                Position = Projectile.position,
                Color = Color.White,
                Rotation = rotation,
                Origin = new Vector2(0, texture0.Height / 2f),
                Scale = new Vector2(Projectile.scale / texture0.Width, Scale * 22f / texture0.Height),
                SpriteEffects = SpriteEffects.None,
                Effect = effect,
            })
            .ApplyOutline(_mainColor)
            .ApplyBloom(1.5f)
            .End();

        Main.spriteBatch.End(out var ss);
        Main.spriteBatch.Begin(ss with { BlendState = BlendState.Additive });

        Main.spriteBatch.Draw(
            glowTexture,
            Projectile.position - Main.screenPosition,
            null,
            _highlightColor * 0.6f,
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
            _highlightColor,
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
            _highlightColor * 0.25f,
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
            _highlightColor * 0.3f,
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
            _highlightColor * 0.6f,
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
